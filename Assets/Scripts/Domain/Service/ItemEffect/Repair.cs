using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor)
        {
            var disabledItems = actor.Inventory.AllItems.Where(item => item.RemainingUses.CurrentValue == item.MaxUsages);
            return disabledItems.Select(item => actor.Inventory.GetItemIndex(item));
        }

        public void Apply(IHasInventory player, IItem item, ItemDatabase itemDatabase)
        {
            item.Repair(player, itemDatabase);
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