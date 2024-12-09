#nullable enable
using Domain.Model.Item;
using View.UI;

namespace Provider
{
    public static class InventoryViewIndexExtensions
    {
        public static InventoryViewIndex ToInventoryViewIndex(this ItemFocus focus) => new(focus.Index, focus.SubIndex, focus.IsGroundItem, focus.IsEmpty);
        public static ItemFocus ToItemFocus(this InventoryViewIndex index) => new(index.Index, index.SubIndex, index.IsGroundItem, index.IsEmpty);
    }
}