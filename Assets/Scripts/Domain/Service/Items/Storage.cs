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

namespace Domain.Service.Items
{
    internal class Storage : IStorage, IDisposable, ISerializable<StorageMemento>
    {
        public int Capacity { get; init; }
        private readonly ObservableList<IItem> _items;
        public IEnumerable<IItem> AllItems => _items
            .WhereNotNull();
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _items
            .Select((item, index) => (item, index))
            .Where(x => x.item != null)
            .Cast<(IItem, int)>();
        public bool CanAddItem { get; init; }
        public bool CanRemoveItem { get; init; }
        public Observable<OnItemInserted> OnItemInserted => _items.ObserveAdd().Select(itemAdded =>
            new OnItemInserted(itemAdded.Value, itemAdded.Index)
        );
        public Observable<OnItemRemoved> OnItemRemoved => _items.ObserveRemove().Select(itemRemoved =>
            new OnItemRemoved(itemRemoved.Value, itemRemoved.Index)
        );
        public Observable<OnItemReplaced> OnItemReplaced => _items.ObserveReplace().Select(itemChanged =>
            new OnItemReplaced(itemChanged.NewValue, itemChanged.OldValue, itemChanged.Index)
        );
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;
        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables;

        public Storage(StorageMemento data)
        {
            Capacity = data.Capacity;
            _items = new ObservableList<IItem>(data.Items.Select(x => x.Deserialize()));
            CanAddItem = data.CanAddItem;
            CanRemoveItem = data.CanRemoveItem;
            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(Capacity).ToArray();
            _disposable = _items.SubscribeIncludingCurrentItems(itemChanged =>
            {
                var index = _items.IndexOf(itemChanged);
                _disposables[index].Clear();
                if (itemChanged != null)
                {
                    itemChanged.OnItemUpdated.Subscribe(
                        _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged))
                    ).AddTo(_disposables[index]);

                    itemChanged.RemainingUses.SkipLatestValueOnSubscribe().Subscribe(
                        remainingUses =>
                        {
                            if (remainingUses <= 0 && itemChanged.AutoDestroyWhenDisabled)
                            {
                                ForceRemove(itemChanged);
                            }
                        }
                    ).AddTo(_disposables[index]);
                }
            });
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
                Capacity,
                _items.Select(x => x.Serialize()).ToList(),
                CanAddItem,
                CanRemoveItem
            );
        }

        public static StorageMemento Build(int capacity, List<IItemMemento> items, bool canAddItemsWithStorage, bool canRemoveItem)
        {
            return new StorageMemento(capacity, items, canAddItemsWithStorage, canRemoveItem);
        }

        public bool HasEmptySpace()
        {
            return _items.Count < Capacity;
        }

        public bool HasItem(IItem item)
        {
            return _items.IndexOf(item) >= 0;
        }

        public bool HasItemAt(int index)
        {
            return index < Capacity;
        }

        public bool HasItemAt(int index, out IItem item)
        {
            item = GetItem(index);
            return index < Capacity;
        }

        public bool Contains(IItem item)
        {
            return _items.IndexOf(item) >= 0;
        }

        public IItem? GetItem(int index)
        {
            return index < _items.Count ? _items[index] : null;
        }

        public int? GetItemIndex(IItem item)
        {
            var index = _items.IndexOf(item);
            return index >= 0 ? index : null;
        }

        public bool CanAddToEmpty(IItem item)
        {
            return CanAddItem && HasEmptySpace();
        }

        public bool CanInsert(IItem item, int index)
        {
            return CanAddItem && HasEmptySpace() && index >= 0 && index < Capacity;
        }

        public bool CanRemove(int index)
        {
            return CanRemoveItem && GetItem(index) != null;
        }

        public bool CanRemove(IItem item)
        {
            return CanRemoveItem && Contains(item);
        }

        public bool CanReplace(IItem item, int index)
        {
            return CanAddItem && CanRemove(index);
        }

        public void AddToEmpty(IItem item)
        {
            if (!CanAddToEmpty(item))
                throw new Exception("Can't add item to storage");
            _items.Add(item);
        }

        public void Insert(IItem item, int index)
        {
            if (!CanInsert(item, index))
                throw new Exception("Can't insert item to storage");
            _items.Insert(index, item);
        }

        public IItem Remove(int index)
        {
            if (!CanRemove(index))
                throw new Exception("Can't remove item from storage");
            var removed = GetItem(index);
            _items.RemoveAt(index);
            return removed;
        }

        public void Remove(IItem item)
        {
            if (!CanRemove(item))
                throw new Exception("Can't remove item from storage");
            _items.Remove(item);
        }

        public IItem Replace(IItem item, int index)
        {
            if (!CanReplace(item, index))
                throw new Exception("Can't replace item from storage");
            var removed = Remove(index);
            Insert(item, index);
            return removed;
        }

        public IItem? ForceRemove(int index)
        {
            var removed = GetItem(index);
            _items.RemoveAt(index);
            return removed;
        }

        public void ForceRemove(IItem item)
        {
            _items.Remove(item);
        }

        public bool CanSwap(int index1, int index2)
        {
            var item1 = GetItem(index1);
            var item2 = GetItem(index2);
            if (item1 == null || item2 == null)
                return false;
            return CanRemove(index1) && CanReplace(item1, index2) && CanInsert(item2, index1);
        }

        public void Swap(int index1, int index2)
        {
            var item1 = Remove(index1);
            var item2 = Replace(item1, index2);
            Insert(item2, index1);
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