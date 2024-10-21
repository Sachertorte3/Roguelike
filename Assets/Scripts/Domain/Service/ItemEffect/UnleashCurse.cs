using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class UnleashCurse : IItemEffect
    {
        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor)
        {
            var disabledItems = actor.Inventory.AllItems.Where(item => !item.IsCursed && (actor.IsKnownItem(item) || item.IsCurseIdentified));
            return disabledItems.Select(item => actor.Inventory.GetItemIndex(item));
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