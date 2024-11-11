#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Items
{
    public record ItemFocus(int index, int subIndex, bool isGroundItem, bool isEmpty)
    {
        public IItem? GetItem(IInventory inventory, IMap map)
        {
            if (isEmpty)
                return null;
            if (isGroundItem)
                return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            var item = inventory.GetItem(index);
            if (subIndex == -1)
                return item;
            else
                return item?.ItemStorage.Value?.GetItem(subIndex);
        }
    }
}