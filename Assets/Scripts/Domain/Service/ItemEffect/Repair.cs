using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public IEnumerable<int> GetDisabledItemIndexes(IInventory inventory)
        {
            var disabledItems = inventory.AllItems.Where(item => item.RemainingUses.CurrentValue == item.MaxUsages);
            return disabledItems.Select(item => inventory.GetItemIndex(item));
        }

        public void Apply(IItem item)
        {
            item.Repair();
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