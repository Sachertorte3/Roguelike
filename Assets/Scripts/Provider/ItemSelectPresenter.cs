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
            var disposable2 = new SerialDisposable();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                disposable.Disposable = map.Player.Character.OnStartItemSelect.Subscribe(message =>
                {
                    itemSelectText.Show(message.Text);
                    inventoryView.LockItems(message.DisabledItemIndexes.Select(index => index.ToInventoryViewIndex()).ToList());
                    inventoryView.SetCanSkip(true);
                });
                disposable2.Disposable = map.Player.Character.OnSelectedItemSelect.Subscribe(message =>
                {
                    itemSelectText.Hide();
                    inventoryView.UnlockAllItems();
                    inventoryView.SetCanSkip(false);
                });
            });
        }
    }
}