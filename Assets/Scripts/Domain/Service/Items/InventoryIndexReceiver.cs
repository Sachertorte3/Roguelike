#nullable enable

namespace Domain.Service.Items
{
    public class InventoryIndexReceiver
    {
        public int? Index { get; private set; }

        public void SetIndex(int? index)
        {
            Index = index;
        }
    }
}