#nullable enable
using Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class ItemSelectPresenter
    {
        [Inject]
        public ItemSelectPresenter(World world, ItemSelectText itemSelectText, InventoryView inventoryView)
        {
            var disposable = new SerialDisposable();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                disposable.Disposable = map.Player.Character.OnItemSelect.Subscribe(message =>
                {
                    if (message.IsWaiting)
                    {
                        itemSelectText.Show();
                        inventoryView.DisableItems(message.DisabledItemIds);
                    }
                    else
                    {
                        itemSelectText.Hide();
                        inventoryView.EnableAllItems();
                    }
                });
            });
        }
    }
}