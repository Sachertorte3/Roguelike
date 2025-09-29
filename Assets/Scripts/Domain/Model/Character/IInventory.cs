#nullable enable
using System.Collections.Generic;
using Domain.Model.Item;

namespace Domain.Model.Character
{
    public interface IInventory : IStorage
    {
        public IEnumerable<ItemFocus> AllIndexesRecursive { get; }
        public IEnumerable<(IItem Item, ItemFocus Index)> AllItemsWithIndexRecursive { get; }
        public bool HasItemAt(ItemFocus index);
        public bool HasItemAt(ItemFocus index, out IItem item);
        public IItem? GetItem(ItemFocus index);
        public ItemFocus? GetItemIndexRecursive(IItem item);
        public bool CanAdd(IItem item, ItemFocus index);
        public bool CanAddOrNot(IItem? item, ItemFocus index);
        public bool CanRemove(ItemFocus index);
        public bool CanReplace(IItem item, ItemFocus index);
        public bool CanReplaceOrRemove(IItem? item, ItemFocus index);
        public void Add(IItem item, ItemFocus index);
        public void AddOrNot(IItem? item, ItemFocus index);
        public IItem? Remove(ItemFocus index);
        public IItem? Replace(IItem item, ItemFocus index);
        public IItem? ReplaceOrRemove(IItem? item, ItemFocus index);
    }
}