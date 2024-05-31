#nullable enable
using Model;
using Model.Game;
using R3;
using VContainer;
using View.UI;
using Utilities;
using System.Diagnostics;
using System.Linq;

namespace Provider
{
    public class PlayerInventoryPresenter
    {
        private readonly SerialDisposable[] _disposables = EnumerableExtension.CreateArrayWithNewInstances<SerialDisposable>(2).ToArray();
        [Inject]
        public PlayerInventoryPresenter(World world, InventoryView inventoryView)
        {
            world.ActiveMap.SubscribeToAll(map =>
            {
                _disposables[0].Disposable = map.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
                {
                    if (itemChanged.NewValue != null)
                        inventoryView.Replace(itemChanged.NewValue.Icon, itemChanged.NewValue.RemainingUses.CurrentValue,
                            itemChanged.NewValue.Info, itemChanged.Index);
                    else
                        inventoryView.Remove(itemChanged.Index);
                });
                _disposables[1].Disposable = map.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                {
                    inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
                });
            });
        }
    }
}