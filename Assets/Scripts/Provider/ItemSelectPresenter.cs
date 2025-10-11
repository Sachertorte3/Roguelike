#nullable enable
using System.Linq;
using Game;
using R3;
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
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var player = mapChanged.Map.Player.Character;
                disposable.Disposable = player.OnStartItemSelect.Subscribe(message =>
                {
                    itemSelectText.Show(message.Text);
                    inventoryView.LockItems(message.DisabledItemIndexes.Select(index => index.ToInventoryViewIndex()).ToList());
                    inventoryView.SetCanSkip(true);
                });
                disposable2.Disposable = player.OnSelectedItemSelect.Subscribe(message =>
                {
                    itemSelectText.Hide();
                    inventoryView.UnlockAllItems();
                    inventoryView.SetCanSkip(false);
                });
            });
        }
    }
}