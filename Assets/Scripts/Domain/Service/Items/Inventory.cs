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
using ObservableCollections;
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
        public int Capacity => _storage.Capacity;
        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged => _storage.OnItemChanged;
        public Observable<OnItemUpdated> OnItemUpdated => _storage.OnItemUpdated;

        public Inventory(StorageMemento data, IHasCondition hasCondition)
        {
            _storage = new Storage(data);
            _hasCondition = hasCondition;
            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(_storage.Capacity).ToArray();

            _disposable = _storage.OnItemChanged.Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();

                if (itemChanged.NewValue != null && !itemChanged.NewValue.IsCursed)
                {
                    foreach (var condition in itemChanged.NewValue.PassiveConditions)
                        condition.Inflict(_hasCondition, Id<IEntity>.Empty);
                }

                if (itemChanged.OldValue != null && !itemChanged.OldValue.IsCursed)
                {
                    foreach (var condition in itemChanged.OldValue.PassiveConditions)
                        condition.Delete(_hasCondition, Id<IEntity>.Empty);
                }

                if (itemChanged.NewValue != null)
                {
                    itemChanged.NewValue.OnCursedChanged.Subscribe(
                        isCursed =>
                        {
                            if (isCursed)
                                foreach (var condition in itemChanged.NewValue.PassiveConditions)
                                    condition.Delete(_hasCondition, Id<IEntity>.Empty);
                            else
                                foreach (var condition in itemChanged.NewValue.PassiveConditions)
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

        public IItem? GetItem(int index, int subIndex)
        {
            if (subIndex < 0)
                return _storage.GetItem(index);
            return _storage.GetItem(index).ItemStorage.Value.GetItem(subIndex);
        }

        public int GetItemIndex(IItem item)
        {
            return _storage.GetItemIndex(item);
        }

        public bool TryAdd(IItem item)
        {
            return _storage.TryAdd(item);
        }

        public bool TryRemove(IItem item)
        {
            return _storage.TryRemove(item);
        }

        public Result<IItem?> Replace(IItem? item, int index, int subIndex)
        {
            if (subIndex < 0)
                return _storage.Replace(item, index);
            return _storage.GetItem(index).ItemStorage.Value.Replace(item, subIndex);
        }

        public Result<IItem?> Replace(IItem? item, int index)
        {
            return _storage.Replace(item, index);
        }
    }
}