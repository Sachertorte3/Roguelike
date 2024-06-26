#nullable enable

namespace Domain.Service.Items
{
    public class InventoryIndexReceiver
    {
        public int Index { get; private set; } = -1;

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}