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
        public static readonly InventoryViewIndex GroundItem = new(InventoryView.GroundItemIndex, -1, true, false);
        public static readonly InventoryViewIndex Empty = new(InventoryView.EmptyIndex, -1, false, true);
        public override string ToString()
        {
            if (this == GroundItem)
                return $"GroundItem";
            else if (this == Empty)
                return $"Empty";
            else if (!IsGroundItem && !IsEmpty)
            {
                if (SubIndex == -1)
                    return $"MainIndex: {Index}";
                else
                    return $"MainIndex: {Index}, SubIndex: {SubIndex}";
            }
            else
                return $"Error";
        }
    }
}