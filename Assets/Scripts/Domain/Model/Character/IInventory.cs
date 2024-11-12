#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Character
{
    public interface IInventory : IStorage
    {
        public IItem? GetItem(int index, int subIndex);
        public IItem? Replace(IItem? item, int index, int subIndex);
    }
}