#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Character
{
    public interface IHasInventory
    {
        public IInventory Inventory { get; }
        public IItemSelector ItemSelector { get; }
        public void AddKnownItem(IItem item);
        public bool IsKnownItem(IItem item);
    }
}