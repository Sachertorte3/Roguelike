#nullable enable
namespace Domain.Model.Item
{
    public interface IHasInventory
    {
        public IInventory Inventory { get; }
        public IItemSelector ItemSelector { get; }
        public void AddKnownItem(IItem item);
        public bool IsKnownItem(IItem item);
    }
}