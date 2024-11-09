using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Identify : IItemEffect
    {
        public bool CanApplyTo(IHasInventory actor, IItem item)
        {
            return !actor.IsKnownItem(item);
        }

        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders)
        {
            actor.AddKnownItem(item);
            item.SetCurseIdentified(true);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Info()
        {
            return "識別";
        }
    }
}