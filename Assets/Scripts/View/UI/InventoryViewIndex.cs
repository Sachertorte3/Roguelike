#nullable enable
namespace View.UI
{
    public record InventoryViewIndex
    {
        public int Index { get; init; }
        public int SubIndex { get; init; }
        public bool IsGroundItem { get; init; }
        public bool IsEmpty { get; init; }
        public InventoryViewIndex(int index, int subIndex, bool isGroundItem, bool isEmpty)
        {
            Index = index;
            SubIndex = subIndex;
            IsGroundItem = isGroundItem;
            IsEmpty = isEmpty;
        }
        public InventoryViewIndex(int index)
        {
            Index = index;
            SubIndex = -1;
            IsGroundItem = index == 10;
            IsEmpty = index == 11;
        }
        public InventoryViewIndex(int index, int subIndex)
        {
            Index = index;
            SubIndex = subIndex;
            IsGroundItem = index == 10;
            IsEmpty = index == 11;
        }
        public static readonly InventoryViewIndex GroundItem = new(10, -1, true, false);
        public static readonly InventoryViewIndex Empty = new(11, -1, false, true);
    }
}