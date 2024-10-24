using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class UnleashCurse : IItemEffect
    {
        public bool CanApplyTo(IHasInventory actor, IItem item)
        {
            return item.IsCursed || (!actor.IsKnownItem(item) && !item.IsCurseIdentified);
        }

        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders)
        {
            item.SetCursed(actor, itemPlaceholders, false);
        }

        public float EvaluatePrice()
        {
            return 200;
        }

        public string Info()
        {
            return "解呪";
        }
    }
}