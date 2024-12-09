#nullable enable
using Domain.Model.Item;
using View.UI;

namespace Provider
{
    public static class InventoryViewIndexExtensions
    {
        public static InventoryViewIndex ToInventoryViewIndex(this ItemFocus focus)
        {
            if (focus == ItemFocus.GroundItem)
                return InventoryViewIndex.GroundItem;
            else if (focus == ItemFocus.Empty)
                return InventoryViewIndex.Empty;
            else
                return new(focus.Index, focus.SubIndex);
        }
        public static ItemFocus ToItemFocus(this InventoryViewIndex index)
        {
            if (index == InventoryViewIndex.GroundItem)
                return ItemFocus.GroundItem;
            else if (index == InventoryViewIndex.Empty)
                return ItemFocus.Empty;
            else
                return new(index.Index, index.SubIndex);
        }
    }
}