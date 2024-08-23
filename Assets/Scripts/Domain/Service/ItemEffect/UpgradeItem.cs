using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;

public class UpgradeItem : IItemEffect
{
    public IEnumerable<int> GetDisabledItemIndexes(IInventory inventory)
    {
        var disabledItems = inventory.AllItems.Where(item => !item.CanUpgrade());
        return disabledItems.Select(item => inventory.GetItemIndex(item));
    }
    public void Apply(IItem item)
    {
        item.Upgrade();
    }
    public string Info() => "強化";
}