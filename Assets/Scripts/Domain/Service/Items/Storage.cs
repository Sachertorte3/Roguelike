#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;
using Utilities.Serialize.Option;
using Utilities.Serialize.Result;
using Result = Utilities.Serialize.Result.Result;

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
        private readonly bool _canAddItemsWithStorage;
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        public Observable<OnItemChanged> OnItemChanged => _items.ObserveReplace().Select(itemChanged =>
            new OnItemChanged(itemChanged.OldValue, itemChanged.NewValue, itemChanged.Index)
        );
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;

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
            _canAddItemsWithStorage = data.CanAddItemsWithStorage;

            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(Capacity).ToArray();
            _disposable = _items.ObserveReplace().Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();
                if (itemChanged.NewValue != null)
                {
                    itemChanged.NewValue.OnItemUpdated.Subscribe(
                        _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index))
                    ).AddTo(_disposables[itemChanged.Index]);

                    itemChanged.NewValue.RemainingUses.Subscribe(
                        remainingUses =>
                        {
                            if (remainingUses <= 0 && itemChanged.NewValue.AutoDestroyWhenDisabled)
                                Remove(itemChanged.Index);
                        }
                    ).AddTo(_disposables[itemChanged.Index]);
                }
            });

            foreach (var (item, i) in data.Items.Index())
            {
                Replace(data.Items[i].Map(item => item.Deserialize()).Value, i);
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
                _canAddItemsWithStorage
            );
        }

        public static StorageMemento Build(IItem?[] items, bool canAddItemsWithStorage)
        {
            return new StorageMemento(
                items.Select(item => item.ToOption().Map(item => item.Serialize())).ToList(),
                canAddItemsWithStorage
            );
        }

        public static StorageMemento Build(int capacity, bool canAddItemsWithStorage)
        {
            var items = EnumerableExtension.CreateNewInstances<Option<IItemMemento>>(capacity).ToList();
            return new StorageMemento(items, canAddItemsWithStorage);
        }

        public bool HasEmptySpace()
        {
            return _items.IndexOf(null) >= 0;
        }

        public IItem? GetItem(int index)
        {
            return _items[index];
        }

        public int GetItemIndex(IItem item)
        {
            return _items.IndexOf(item);
        }

        public void Add(IItem item)
        {
            if (!TryAdd(item))
                throw new Exception("Can't add item to storage");
        }

        public bool TryAdd(IItem item)
        {
            if (!_canAddItemsWithStorage && item.ItemStorage.IsSome)
                return false;
            var index = _items.IndexOf(null);
            if (index >= 0)
            {
                Replace(item, index);
                return true;
            }

            return false;
        }

        public void Remove(IItem item)
        {
            if (!TryRemove(item))
                throw new Exception("Can't remove item from storage");
        }

        public bool TryRemove(IItem item)
        {
            var index = _items.IndexOf(item);
            if (index >= 0)
            {
                Remove(index);
                return true;
            }
            return false;
        }

        public Result<IItem?> Replace(IItem? item, int index)
        {
            if (!_canAddItemsWithStorage && item != null && item.ItemStorage.IsSome)
                return Result<IItem?>.Error;
            var removed = _items[index];

            if (item != null || removed != null)
            {
                _items[index] = item;
            }
            return Result.Ok(removed);
        }

        public IItem? Remove(int index)
        {
            return Replace(null, index).Value;
        }

        public IEnumerable<IItem> Clear()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = Replace(null, i).Value;
                if (item != null)
                    yield return item;
            }
        }
    }
}