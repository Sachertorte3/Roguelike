using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Identify : IItemEffect
    {
        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor)
        {
            var disabledItems = actor.Inventory.AllItems.Where(item => actor.IsKnownItem(item));
            return disabledItems.Select(item => actor.Inventory.GetItemIndex(item));
        }

        public void Apply(IHasInventory actor, IItem item)
        {
            actor.AddKnownItem(item);
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