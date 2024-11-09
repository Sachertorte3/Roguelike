#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;

namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        public bool CanApplyTo(IPlayer player, IItem item);
        public void Apply(IPlayer player, IItem item, ItemPlaceholders itemPlaceholders);
        public float EvaluatePrice();
    }
}