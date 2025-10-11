#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
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

        private ICharacter _character;

        public IEnumerable<IItem> AllItems => _storage.AllItems;
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _storage.AllItemsWithIndex;
        public int Capacity => _storage.Capacity;
        public bool CanAddItem => _storage.CanAddItem;
        public bool CanRemoveItem => _storage.CanRemoveItem;
        public Observable<OnItemInserted> OnItemInserted => _storage.OnItemInserted;
        public Observable<OnItemRemoved> OnItemRemoved => _storage.OnItemRemoved;
        public Observable<OnItemReplaced> OnItemReplaced => _storage.OnItemReplaced;
        public Observable<OnItemUpdated> OnItemUpdated => _storage.OnItemUpdated;

        public Inventory(StorageMemento data, ICharacter character)
        {
            _storage = new Storage(data);
            _character = character;
            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(_storage.Capacity).ToArray();

            _disposable = _storage.OnItemInserted.Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();

                if (itemChanged.NewItem != null && !itemChanged.NewItem.IsCursed)
                {
                    foreach (var condition in itemChanged.NewItem.PassiveConditions)
                        condition.Inflict(_character, Id<IEntity>.Empty);
                }

                if (itemChanged.NewItem != null)
                {
                    itemChanged.NewItem.OnCursedChanged.Subscribe(
                        isCursed =>
                        {
                            if (isCursed)
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Delete(_character, Id<IEntity>.Empty);
                            else
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Inflict(_character, Id<IEntity>.Empty);
                        }
                    ).AddTo(_disposables[itemChanged.Index]);
                }
            });
            _disposable = _storage.OnItemRemoved.Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();

                if (itemChanged.OldItem != null && !itemChanged.OldItem.IsCursed)
                {
                    foreach (var condition in itemChanged.OldItem.PassiveConditions)
                        condition.Delete(_character, Id<IEntity>.Empty);
                }
            });
            _disposable = _storage.OnItemReplaced.Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();

                if (itemChanged.NewItem != null && !itemChanged.NewItem.IsCursed)
                {
                    foreach (var condition in itemChanged.NewItem.PassiveConditions)
                        condition.Inflict(_character, Id<IEntity>.Empty);
                }

                if (itemChanged.OldItem != null && !itemChanged.OldItem.IsCursed)
                {
                    foreach (var condition in itemChanged.OldItem.PassiveConditions)
                        condition.Delete(_character, Id<IEntity>.Empty);
                }

                if (itemChanged.NewItem != null)
                {
                    itemChanged.NewItem.OnCursedChanged.Subscribe(
                        isCursed =>
                        {
                            if (isCursed)
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Delete(_character, Id<IEntity>.Empty);
                            else
                                foreach (var condition in itemChanged.NewItem.PassiveConditions)
                                    condition.Inflict(_character, Id<IEntity>.Empty);
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
                    condition.Persist(_character);
                }
            }
        }

        public StorageMemento Serialize() => _storage.Serialize();
        public bool HasEmptySpace() => _storage.HasEmptySpace();
        public bool Contains(IItem item) => _storage.Contains(item);
        public int? GetItemIndex(IItem item) => _storage.GetItemIndex(item);
        public bool HasItem(IItem item) => _storage.HasItem(item);
        public bool HasItemAt(int index) => _storage.HasItemAt(index);
        public bool HasItemAt(int index, out IItem item) => _storage.HasItemAt(index, out item);
        public IItem? GetItem(int index) => _storage.GetItem(index);
        public bool CanAddToEmpty(IItem item) => _storage.CanAddToEmpty(item);
        public void AddToEmpty(IItem item)
        {
            if (_storage.CanAddToEmpty(item))
            {
                _storage.AddToEmpty(item);
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
            }
            else
                throw new Exception("Can't add item to inventory");
        }
        public bool CanAddOrNot(IItem? item)
        {
            if (item == null)
                return true;
            return CanAddToEmpty(item);
        }
        public void AddOrNot(IItem? item)
        {
            if (item == null)
                return;
            AddToEmpty(item);
        }
        public bool CanRemove(IItem item) => _storage.CanRemove(item);
        public void Remove(IItem item) => _storage.Remove(item);
        public bool CanRemove(int index) => _storage.CanRemove(index);
        public IItem Remove(int index) => _storage.Remove(index);
        public bool CanReplace(IItem item, int index) => _storage.CanReplace(item, index);
        public IItem Replace(IItem item, int index)
        {
            if (_storage.CanReplace(item, index))
            {
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
                var replacedItem = _storage.Replace(item, index);
                return replacedItem;
            }
            else
                throw new Exception("Can't replace item in inventory");
        }
        public bool CanSwap(int index1, int index2) => _storage.CanSwap(index1, index2);
        public void Swap(int index1, int index2) => _storage.Swap(index1, index2);
        public bool CanReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return CanRemove(index);
            return CanReplace(item, index);
        }
        public IItem ReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return Remove(index);
            return Replace(item, index);
        }
        public IEnumerable<IItem> Clear() => _storage.Clear();
    }
}