using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public bool CanApplyTo(IHasInventory actor, IItem item)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        public void Apply(IHasInventory player, IItem item, ItemPlaceholders itemPlaceholders)
        {
            item.Repair(player, itemPlaceholders);
        }

        public float EvaluatePrice()
        {
            return 500;
        }

        public string Info()
        {
            return "修理";
        }
    }
}