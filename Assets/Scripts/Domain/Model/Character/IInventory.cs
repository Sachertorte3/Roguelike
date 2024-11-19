#nullable enable
using Domain.Model.Item;
using Utilities.Serialize.Result;

namespace Domain.Model.Character
{
    public interface IInventory : IStorage
    {
        public IItem? GetItem(int index, int subIndex);
        public Result<IItem?> Replace(IItem? item, int index, int subIndex);
    }
}