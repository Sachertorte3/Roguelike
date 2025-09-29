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
            .SelectMany(main =>
            {
                var storage = _storage.GetItemStorage(main);
                if (storage != null)
                    return Enumerable.Range(0, storage.Capacity)
                        .Select(sub => new ItemFocus(main, sub))
                        .Append(new ItemFocus(main));
                else
                    return new[] { new ItemFocus(main) };
            });

        public int Capacity => _storage.Capacity;
        public bool CanRemoveItem => _storage.CanRemoveItem;
        public Observable<OnItemChanged> OnItemChanged => _storage.OnItemChanged;
        public Observable<OnItemUpdated> OnItemUpdated => _storage.OnItemUpdated;
        public Observable<OnItemOverflowed> OnItemOverflowed => _storage.OnItemOverflowed;

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

        public StorageMemento Serialize() => _storage.Serialize();
        public bool HasEmptySpace() => _storage.HasEmptySpace();
        private T ExecuteOnStorage<T>(ItemFocus index, Func<IStorage, int, T> func)
        {
            if (index.SubIndex < 0)
                return func(_storage, index.Index);
            var itemStorage = _storage.GetItemStorage(index.Index);
            if (itemStorage == null)
                throw new Exception("Index is not valid");
            return func(itemStorage, index.SubIndex);
        }
        private void ExecuteOnStorage(ItemFocus index, Action<IStorage, int> action)
        {
            if (index.SubIndex < 0)
                action(_storage, index.Index);
            var itemStorage = _storage.GetItemStorage(index.Index);
            if (itemStorage == null)
                throw new Exception("Index is not valid");
            action(itemStorage, index.SubIndex);
        }
        public bool HasItemAt(ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.HasItemAt(index));
        }
        public bool HasItemAt(ItemFocus index, out IItem item)
        {
            IItem? tempItem = null;
            var result = ExecuteOnStorage(index, (storage, index) => storage.HasItemAt(index, out tempItem));
            item = tempItem;
            return result;
        }
        public bool Contains(IItem item) => _storage.Contains(item);
        public IItem? GetItem(ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.GetItem(index));
        }
        public int GetItemIndex(IItem? item) => _storage.GetItemIndex(item);
        public ItemFocus? GetItemIndexRecursive(IItem item)
        {
            if (AllItemsWithIndexRecursive.Any(x => x.Item == item))
                return AllItemsWithIndexRecursive.First(x => x.Item == item).Index;
            return null;
        }
        public bool CanAddToEmpty(IItem item) => _storage.CanAddToEmpty(item);
        public bool CanAdd(IItem item, ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.CanAdd(item, index));
        }
        public bool CanAddOrNot(IItem? item, ItemFocus index)
        {
            if (item == null)
                return true;
            return CanAdd(item, index);
        }
        public bool CanRemove(ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.CanRemove(index));
        }
        public bool CanRemove(IItem item) => _storage.CanRemove(item);
        public bool CanReplace(IItem item, ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.CanReplace(item, index));
        }
        public bool CanReplaceOrRemove(IItem? item, ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.CanReplaceOrRemove(item, index));
        }
        public void AddToEmpty(IItem item) => _storage.AddToEmpty(item);
        public void Add(IItem item, ItemFocus index)
        {
            ExecuteOnStorage(index, (storage, index) => storage.Add(item, index));
        }
        public void AddOrNot(IItem? item, ItemFocus index)
        {
            if (item == null)
                return;
            CanAdd(item, index);
        }
        public IItem? Remove(ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.Remove(index));
        }
        public void Remove(IItem item)
        {
            if (_storage.Contains(item))
            {
                _storage.Remove(item);
                return;
            }

            var storages = _storage.AllItems
                .Where(x => x.ItemStorage.IsSome())
                .Select(x => x.ItemStorage.Value);

            storages.First(storage => storage.Contains(item)).Remove(item);
        }
        public IItem? Replace(IItem? item, ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.Replace(item, index));
        }
        public IItem? ReplaceOrRemove(IItem? item, ItemFocus index)
        {
            return ExecuteOnStorage(index, (storage, index) => storage.ReplaceOrRemove(item, index));
        }
        public bool HasItemAt(int index) => _storage.HasItemAt(index);
        public bool HasItemAt(int index, out IItem item) => _storage.HasItemAt(index, out item);
        public IItem? GetItem(int index) => _storage.GetItem(index);
        public IStorage? GetItemStorage(int index) => _storage.GetItemStorage(index);
        public bool CanAdd(IItem item, int index) => _storage.CanAdd(item, index);
        public void Add(IItem item, int index) => _storage.Add(item, index);
        public bool CanAddOrNot(IItem? item, int index) => _storage.CanAddOrNot(item, index);
        public void AddOrNot(IItem? item, int index) => _storage.AddOrNot(item, index);
        public bool CanRemove(int index) => _storage.CanRemove(index);
        public IItem? Remove(int index) => _storage.Remove(index);
        public bool CanReplace(IItem item, int index) => _storage.CanReplace(item, index);
        public IItem? Replace(IItem item, int index) => _storage.Replace(item, index);
        public bool CanReplaceOrRemove(IItem? item, int index) => _storage.CanReplaceOrRemove(item, index);
        public IItem? ReplaceOrRemove(IItem? item, int index) => _storage.ReplaceOrRemove(item, index);
        public IEnumerable<IItem> Clear() => _storage.Clear();
    }
}