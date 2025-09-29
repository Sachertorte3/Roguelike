#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    internal class Storage : IStorage, IDisposable, ISerializable<StorageMemento>
    {
        public int Capacity => _items.Count;
        private readonly ObservableList<IItem?> _items;
        public IEnumerable<IItem> AllItems => _items
            .WhereNotNull();
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _items
            .Select((item, index) => (item, index))
            .Where(x => x.item != null)
            .Cast<(IItem, int)>();
        public IEnumerable<IItem> AllItemsRecursive => AllItems
            .SelectMany(x =>
                x.ItemStorage
                    .MapOr(Enumerable.Empty<IItem>(), storage => storage.AllItemsRecursive)
                    .Append(x)
            );
        public bool CanAddItemsWithStorage { get; init; }
        public bool CanRemoveItem { get; init; }
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();
        private readonly Subject<OnItemOverflowed> _onItemOverflowed = new();
        public Observable<OnItemChanged> OnItemChanged => _items.ObserveReplace().Select(itemChanged =>
            new OnItemChanged(itemChanged.OldValue, itemChanged.NewValue, itemChanged.Index)
        );
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;
        public Observable<OnItemOverflowed> OnItemOverflowed => _onItemOverflowed;
        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables;

        public Storage(StorageMemento data)
        {
            _items = new ObservableList<IItem?>();
            for (var i = 0; i < data.Items.Count; i++)
            {
                //MEMO: Since you are only subscribed to Replace, additions must also be done using Replace.
                _items.Add(null);
            }
            CanAddItemsWithStorage = data.CanAddItemsWithStorage;
            CanRemoveItem = data.CanRemoveItem;
            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(Capacity).ToArray();
            _disposable = _items.SubscribeIncludingCurrentItems(itemChanged =>
            {
                var index = _items.IndexOf(itemChanged);
                _disposables[index].Clear();
                if (itemChanged != null)
                {
                    itemChanged.OnItemUpdated.Subscribe(
                        _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged, index))
                    ).AddTo(_disposables[index]);

                    if (itemChanged.ItemStorage.IsSome(out var itemStorage))
                    {
                        itemStorage.OnItemOverflowed.Subscribe(
                            items => _onItemOverflowed.OnNext(items)
                        ).AddTo(_disposables[index]);
                    }

                    itemChanged.RemainingUses.SkipLatestValueOnSubscribe().Subscribe(
                        remainingUses =>
                        {
                            if (remainingUses <= 0 && itemChanged.AutoDestroyWhenDisabled)
                            {
                                ForceRemove(index);
                                if (itemChanged.ItemStorage.IsSome(out var itemStorage))
                                {
                                    _onItemOverflowed.OnNext(new OnItemOverflowed(itemChanged, itemStorage.AllItems));
                                }
                            }
                        }
                    ).AddTo(_disposables[index]);
                }
            });

            foreach (var (itemMemento, i) in data.Items.Index())
            {
                if (itemMemento.IsSome(out var item))
                    Replace(item.Deserialize(), i);
            }
        }

        public void Dispose()
        {
            _onItemUpdated.Dispose();
            _disposable.Dispose();
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public StorageMemento Serialize()
        {
            return new StorageMemento
            (
                _items.Select(x => x.ToOption().Map(x => x.Serialize())).ToList(),
                CanAddItemsWithStorage,
                CanRemoveItem
            );
        }

        public static StorageMemento Build(IItem?[] items, bool canAddItemsWithStorage, bool canRemoveItem)
        {
            return new StorageMemento(
                items.Select(item => item.ToOption().Map(item => item.Serialize())).ToList(),
                canAddItemsWithStorage,
                canRemoveItem
            );
        }

        public static StorageMemento Build(int capacity, bool canAddItemsWithStorage, bool canRemoveItem)
        {
            var items = EnumerableExtension.CreateNewInstances<Option<IItemMemento>>(capacity).ToList();
            return new StorageMemento(items, canAddItemsWithStorage, canRemoveItem);
        }

        public bool HasEmptySpace()
        {
            return _items.IndexOf(null) >= 0;
        }

        public bool HasItemAt(int index)
        {
            return _items[index] != null;
        }

        public bool HasItemAt(int index, out IItem item)
        {
            item = _items[index];
            return item != null;
        }

        public bool Contains(IItem item)
        {
            return _items.IndexOf(item) >= 0;
        }

        public IItem? GetItem(int index)
        {
            return _items[index];
        }

        public int GetItemIndex(IItem? item)
        {
            return _items.IndexOf(item);
        }

        public IStorage? GetItemStorage(int index)
        {
            return _items[index]?.ItemStorage.Value;
        }

        public bool CanAddToEmpty(IItem item)
        {
            return (CanAddItemsWithStorage || item.ItemStorage.IsNone) && HasEmptySpace();
        }

        public bool CanAdd(IItem item, int index)
        {
            return (CanAddItemsWithStorage || item.ItemStorage.IsNone) && _items[index] == null;
        }

        public bool CanAddOrNot(IItem? item, int index)
        {
            if (item == null)
                return true;
            return CanAdd(item, index);
        }

        public bool CanRemove(int index)
        {
            return CanRemoveItem || _items[index] == null;
        }

        public bool CanRemove(IItem item)
        {
            return CanRemoveItem && Contains(item);
        }

        public bool CanReplace(IItem item, int index)
        {
            return (CanAddItemsWithStorage || item.ItemStorage.IsNone) && CanRemove(index);
        }

        public bool CanReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return CanRemove(index);
            return CanReplace(item, index);
        }

        public void AddToEmpty(IItem item)
        {
            if (!CanAddItemsWithStorage && item.ItemStorage.IsSome())
                throw new Exception("Can't add item to storage");
            var index = _items.IndexOf(null);
            if (index < 0)
                throw new Exception("No empty space in storage");
            _items[index] = item;
        }

        public void Add(IItem item, int index)
        {
            if (!CanAddItemsWithStorage && item.ItemStorage.IsSome())
                throw new Exception("Can't add item to storage");
            if (_items[index] != null)
                throw new Exception("Item already exists in storage");
            _items[index] = item;
        }

        public void AddOrNot(IItem? item, int index)
        {
            if (item != null)
                Add(item, index);
        }

        public IItem? Remove(int index)
        {
            var removed = _items[index];
            if (!CanRemoveItem && removed != null)
                throw new Exception("Can't remove item from storage");
            _items[index] = null;
            return removed;
        }

        public void Remove(IItem item)
        {
            if (!CanRemoveItem)
                throw new Exception("Can't remove item from storage");
            var index = _items.IndexOf(item);
            if (index < 0)
                throw new Exception("Item not found in storage");
            _items[index] = null;
        }

        public IItem? Replace(IItem item, int index)
        {
            if (!CanAddItemsWithStorage && item.ItemStorage.IsSome())
                throw new Exception("Can't add item to storage");
            var removed = _items[index];
            if (!CanRemoveItem && removed != null)
                throw new Exception("Can't remove item from storage");
            _items[index] = item;
            return removed;
        }

        public IItem? ReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return Remove(index);
            return Replace(item, index);
        }

        public IItem? ForceRemove(int index)
        {
            var removed = _items[index];
            _items[index] = null;
            return removed;
        }

        public IEnumerable<IItem> Clear()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = ForceRemove(i);
                if (item != null)
                    yield return item;
            }
        }
    }
}