#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;

namespace Domain.Model.Item
{
    public interface IInventoryEffect : IHasInfo
    {
        public void Apply(IPlayer player, IStorage storage, ItemPlaceholders itemPlaceholders);
        public float EvaluatePrice();
    }
}