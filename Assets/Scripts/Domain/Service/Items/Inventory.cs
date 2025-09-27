#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using R3;
using Utilities;
using Utilities.Serialize.Result;

namespace Domain.Service.Items
{
    internal class Inventory : IInventory, IDisposable, ISerializable<StorageMemento>
    {
        private readonly Storage _storage;

        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables;

        private IHasCondition _hasCondition;

        public IEnumerable<IItem> AllItems => _storage.AllItems;
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _storage.AllItemsWithIndex;
        public IEnumerable<IItem> AllItemsRecursive => _storage.AllItemsRecursive;
        public IEnumerable<(IItem Item, ItemFocus Index)> AllItemsWithIndexRecursive => AllItemsWithIndex
            .SelectMany(main =>
                main.Item.ItemStorage
                    .MapOr(Enumerable.Empty<(IItem, ItemFocus)>(), storage => storage.AllItemsWithIndex
                        .Select(sub => (sub.Item, new ItemFocus(main.Index, sub.Index)))
                    )
                    .Append((main.Item, new ItemFocus(main.Index)))
            );
        public IEnumerable<ItemFocus> AllIndexesRecursive => Enumerable.Range(0, _storage.Capacity)
            .SelectMany(main => {
                var item = GetItem(main);
                if (item == null || item.ItemStorage.IsNone)
                    return new[] { new ItemFocus(main) };
                return Enumerable.Range(0, item.ItemStorage.Value.Capacity)
                    .Select(sub => new ItemFocus(main, sub))
                    .Append(new ItemFocus(main));
            });

        public int Capacity => _storage.Capacity;
        public bool CanRemoveItem => _storage.CanRemoveItem;
        public Observable<OnItemChanged> OnItemChanged => _storage.OnItemChanged;
        public Observable<OnItemUpdated> OnItemUpdated => _storage.OnItemUpdated;

        public Inventory(StorageMemento data, IHasCondition hasCondition)
        {
            _storage = new Storage(data);
            _hasCondition = hasCondition;
            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(_storage.Capacity).ToArray();

            _disposable = _storage.OnItemChanged.Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();

                if (itemChanged.NewItem != null && !itemChanged.NewItem.IsCursed)
                {
                    foreach (var condition in itemChanged.NewItem.PassiveConditions)
                        condition.Inflict(_hasCondition, Id<IEntity>.Empty);
                }

                if (itemChanged.OldItem != null && !itemChanged.OldItem.IsCursed)
                {
                    foreach (var condition in itemChanged.OldItem.PassiveConditions)
                        condition.Delete(_hasCondition, Id<IEntity>.Empty);
                }

                if (itemChanged.NewItem != null)
                {
                    itemChanged.NewItem.OnCursedChanged.Subscribe(
                        isCursed =>
                        {
                            if (isCursed)
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Delete(_hasCondition, Id<IEntity>.Empty);
                            else
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Inflict(_hasCondition, Id<IEntity>.Empty);
                        }
                    ).AddTo(_disposables[itemChanged.Index]);
                }
            });
        }

        public void Dispose()
        {
            _disposable.Dispose();
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public void UpdateTurn()
        {
            foreach (var item in _storage.AllItems)
            {
                if (item == null)
                    continue;
                foreach (var condition in item.PassiveConditions)
                {
                    condition.Persist(_hasCondition);
                }
            }
        }

        public StorageMemento Serialize()
        {
            return _storage.Serialize();
        }

        public bool HasEmptySpace()
        {
            return _storage.HasEmptySpace();
        }

        public IItem? GetItem(int index)
        {
            return _storage.GetItem(index);
        }

        public IItem? GetItem(ItemFocus index)
        {
            if (index.SubIndex < 0)
                return _storage.GetItem(index.Index);
            return _storage.GetItem(index.Index).ItemStorage.Value.GetItem(index.SubIndex);
        }

        public int GetItemIndex(IItem item)
        {
            return _storage.GetItemIndex(item);
        }

        public ItemFocus? GetItemIndexRecursive(IItem item)
        {
            if (AllItemsWithIndexRecursive.Any(x => x.Item == item))
                return AllItemsWithIndexRecursive.First(x => x.Item == item).Index;
            return null;
        }

        public void Add(IItem item)
        {
            if (!TryAdd(item))
                throw new Exception("Can't add item to inventory");
        }

        public bool TryAdd(IItem item)
        {
            return _storage.TryAdd(item);
        }

        public void Remove(IItem item)
        {
            if (!TryRemove(item))
                throw new Exception("Can't remove item from inventory");
        }

        public bool TryRemove(IItem item)
        {
            if (_storage.TryRemove(item))
                return true;

            var storages = _storage.AllItems
                .Where(x => x.ItemStorage.IsSome)
                .Select(x => x.ItemStorage.Value);

            return storages.Any(storage => storage.TryRemove(item));
        }

        public Result<IItem?> Replace(IItem? item, ItemFocus index)
        {
            if (index.SubIndex < 0)
                return _storage.Replace(item, index.Index);
            var itemStorage = _storage.GetItem(index.Index);
            if (itemStorage == null || itemStorage.ItemStorage.IsNone)
                return Result<IItem?>.Error;
            return itemStorage.ItemStorage.Value.Replace(item, index.SubIndex);
        }

        public Result<IItem?> Replace(IItem? item, int index)
        {
            return _storage.Replace(item, index);
        }

        public IEnumerable<IItem> Clear()
        {
            return _storage.Clear();
        }
    }
}