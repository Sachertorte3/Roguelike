#nullable enable
using System.Collections.Generic;
using Domain.Model.Item;
using Utilities.Serialize.Result;

namespace Domain.Model.Character
{
    public interface IInventory : IStorage
    {
        public IEnumerable<ItemFocus> AllIndexesRecursive { get; }
        public IEnumerable<(IItem Item, ItemFocus Index)> AllItemsWithIndexRecursive { get; }
        public IItem? GetItem(ItemFocus index);
        public ItemFocus GetItemIndexRecursive(IItem item);
        public Result<IItem?> Replace(IItem? item, ItemFocus index);
    }
}