#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Items
{
    public record ItemFocus(int index, bool isGroundItem, bool isEmpty)
    {
        public IItem? GetItem(IInventory inventory, IMap map)
        {
            if (isEmpty)
                return null;
            if (isGroundItem)
                return map.Items.At(map.Player.Entity.CurrentPosition).FirstOrDefault()?.Item;
            return inventory.GetItem(index);
        }
    }
    public class InventoryIndexReceiver
    {
        public ItemFocus Focus { get; private set; }

        public void SetFocus(ItemFocus focus)
        {
            Focus = focus;
        }
    }
}