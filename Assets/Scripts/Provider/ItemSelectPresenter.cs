#nullable enable
using System.Linq;
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
                        itemSelectText.Show(message.Text);
                        inventoryView.LockItems(message.DisabledItemIndexes.Select(index => index.ToInventoryViewIndex()).ToArray());
                        inventoryView.SetCanSkip(true);
                    }
                    else
                    {
                        itemSelectText.Hide();
                        inventoryView.UnlockAllItems();
                        inventoryView.SetCanSkip(false);
                    }
                });
            });
        }
    }
}