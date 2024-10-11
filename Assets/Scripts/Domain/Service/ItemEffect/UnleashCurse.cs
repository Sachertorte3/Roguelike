using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class UnleashCurse : IItemEffect
    {
        public IEnumerable<int> GetDisabledItemIndexes(IInventory inventory)
        {
            var disabledItems = inventory.AllItems.Where(item => !item.IsCursed);
            return disabledItems.Select(item => inventory.GetItemIndex(item));
        }

        public void Apply(IItem item)
        {
            item.SetCursed(false);
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