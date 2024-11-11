#nullable enable
namespace Domain.Service.Items
{
    public class InventoryIndexReceiver
    {
        public ItemFocus Focus { get; private set; }

        public void SetFocus(ItemFocus focus)
        {
            Focus = focus;
        }
    }
}