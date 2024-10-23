#nullable enable
using Domain.Model.Dungeon;

namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        public bool CanApplyTo(IHasInventory actor, IItem item);
        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders);
        public float EvaluatePrice();
    }
}