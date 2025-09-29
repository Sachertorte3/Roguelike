#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Items;
using R3;

namespace Domain.Service.Characters
{
    internal sealed class CharacterInventory : IInventory
    {
        private readonly Inventory _inventory;
        private readonly ICharacter _character;
        public CharacterInventory(StorageMemento data, ICharacter character)
        {
            _inventory = new Inventory(data, character);
            _character = character;
        }

        public IEnumerable<ItemFocus> AllIndexesRecursive => _inventory.AllIndexesRecursive;
        public IEnumerable<(IItem Item, ItemFocus Index)> AllItemsWithIndexRecursive => _inventory.AllItemsWithIndexRecursive;
        public IEnumerable<IItem> AllItems => _inventory.AllItems;
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _inventory.AllItemsWithIndex;
        public IEnumerable<IItem> AllItemsRecursive => _inventory.AllItemsRecursive;
        public int Capacity => _inventory.Capacity;
        public bool CanRemoveItem => _inventory.CanRemoveItem;
        public Observable<OnItemChanged> OnItemChanged => _inventory.OnItemChanged;
        public Observable<OnItemUpdated> OnItemUpdated => _inventory.OnItemUpdated;
        public Observable<OnItemOverflowed> OnItemOverflowed => _inventory.OnItemOverflowed;
        public void Dispose()
        {
            _inventory.Dispose();
        }
        public StorageMemento Serialize() => _inventory.Serialize();
        public void UpdateTurn()
        {
            _inventory.UpdateTurn();
        }
        public bool HasItemAt(int index) => _inventory.HasItemAt(index);
        public bool HasItemAt(int index, out IItem item) => _inventory.HasItemAt(index, out item);
        public IItem? GetItem(int index) => _inventory.GetItem(index);
        public bool CanAdd(IItem item, int index) => _inventory.CanAdd(item, index);
        public void Add(IItem item, int index) => _inventory.Add(item, index);
        public bool CanAddOrNot(IItem? item, int index) => _inventory.CanAddOrNot(item, index);
        public void AddOrNot(IItem? item, int index) => _inventory.AddOrNot(item, index);
        public bool CanRemove(int index) => _inventory.CanRemove(index);
        public IItem? Remove(int index) => _inventory.Remove(index);
        public bool CanReplace(IItem item, int index) => _inventory.CanReplace(item, index);
        public IItem? Replace(IItem item, int index) => _inventory.Replace(item, index);
        public bool CanReplaceOrRemove(IItem? item, int index) => _inventory.CanReplaceOrRemove(item, index);
        public IItem? ReplaceOrRemove(IItem? item, int index) => _inventory.ReplaceOrRemove(item, index);
        public IEnumerable<IItem> Clear() => _inventory.Clear();
        public IItem? GetItem(ItemFocus index) => _inventory.GetItem(index);
        public int GetItemIndex(IItem? item) => _inventory.GetItemIndex(item);
        public ItemFocus? GetItemIndexRecursive(IItem item) => _inventory.GetItemIndexRecursive(item);
        public IStorage? GetItemStorage(int index) => _inventory.GetItemStorage(index);
        public bool HasEmptySpace() => _inventory.HasEmptySpace();
        public bool HasItemAt(ItemFocus index) => _inventory.HasItemAt(index);
        public bool HasItemAt(ItemFocus index, out IItem item) => _inventory.HasItemAt(index, out item);
        public bool Contains(IItem item) => _inventory.Contains(item);
        public bool CanAdd(IItem item, ItemFocus index) => _inventory.CanAdd(item, index);
        public bool CanAddOrNot(IItem? item, ItemFocus index) => _inventory.CanAddOrNot(item, index);
        public bool CanAddToEmpty(IItem item) => _inventory.CanAddToEmpty(item);
        public bool CanRemove(ItemFocus index) => _inventory.CanRemove(index);
        public bool CanRemove(IItem item) => _inventory.CanRemove(item);
        public bool CanReplace(IItem? item, ItemFocus index) => _inventory.CanReplace(item, index);
        public bool CanReplaceOrRemove(IItem? item, ItemFocus index) => _inventory.CanReplaceOrRemove(item, index);
        public void AddToEmpty(IItem item)
        {
            if (_inventory.CanAddToEmpty(item))
            {
                _inventory.AddToEmpty(item);
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
            }
            else
                throw new Exception("Can't add item to inventory");
        }
        public void Add(IItem item, ItemFocus index)
        {
            if (_inventory.CanAdd(item, index))
            {
                _inventory.Add(item, index);
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
            }
            else
                throw new Exception("Can't add item to inventory");
        }
        public void AddOrNot(IItem? item, ItemFocus index)
        {
            if (item != null)
            {
                _inventory.Add(item, index);
            }
        }
        public IItem? Remove(ItemFocus index)
        {
            if (_inventory.CanRemove(index))
                return _inventory.Remove(index);
            else
                throw new Exception("Can't remove item from inventory");
        }
        public void Remove(IItem item)
        {
            if (_inventory.CanRemove(item))
                _inventory.Remove(item);
            else
                throw new Exception("Can't remove item from inventory");
        }
        public IItem? Replace(IItem? item, ItemFocus index)
        {
            if (_inventory.CanReplace(item, index))
            {
                if (item != null && (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue))
                {
                    _character.KnowItem(item, false);
                }
                var replacedItem = _inventory.Replace(item, index);
                return replacedItem;
            }
            else
                throw new Exception("Can't replace item in inventory");
        }
        public IItem? ReplaceOrRemove(IItem? item, ItemFocus index)
        {
            if (item != null)
            {
                return _inventory.Replace(item, index);
            }
            else
            {
                return _inventory.Remove(index);
            }
        }
    }
}