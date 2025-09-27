#nullable enable
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public record ItemFocus
    {
        public int Index { get; init; }
        public int SubIndex { get; init; }
        public bool IsOnGroundItem { get; init; }
        public bool IsOnEmpty { get; init; }
        public ItemFocus(int index, int subIndex, bool isGroundItem, bool isEmpty)
        {
            Index = index;
            SubIndex = subIndex;
            IsOnGroundItem = isGroundItem;
            IsOnEmpty = isEmpty;
        }
        public ItemFocus(int index) : this(index, -1, false, false) { }
        public ItemFocus(int index, int subIndex) : this(index, subIndex, false, false) { }
        public static readonly ItemFocus GroundItem = new(-1, -1, true, false);
        public static readonly ItemFocus Empty = new(-1, -1, false, true);
        public IItem? GetItem(IInventory inventory, IMap map)
        {
            if (IsOnEmpty)
                return null;
            if (IsOnGroundItem)
                return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            var item = inventory.GetItem(Index);
            if (SubIndex == -1)
                return item;
            else
                return item?.ItemStorage.Value?.GetItem(SubIndex);
        }
        public bool IsOnItem(IInventory inventory, IMap map) => GetItem(inventory, map) != null;
        public bool IsOnItem(IInventory inventory, IMap map, out IItem item)
        {
            item = GetItem(inventory, map);
            return item != null;
        }
    }
}