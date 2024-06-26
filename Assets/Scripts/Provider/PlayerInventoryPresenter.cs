#nullable enable
using Model.Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class PlayerInventoryPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public PlayerInventoryPresenter(World world, InventoryView inventoryView)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    _disposables.Add(map.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
                    {
                        if (itemChanged.NewValue != null)
                            inventoryView.Replace(itemChanged.NewValue.Icon,
                                itemChanged.NewValue.RemainingUses.CurrentValue,
                                itemChanged.NewValue.Info(), itemChanged.Index);
                        else
                            inventoryView.Remove(itemChanged.Index);
                    }));
                    _disposables.Add(map.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
                        inventoryView.UpdateInfo(itemUpdated.Item.Info(), itemUpdated.Index);
                    }));
                    for (var i = 0; i < map.Player.Inventory.MaxItemCount; i++)
                    {
                        var item = map.Player.Inventory.GetItem(i);
                        if (item != null)
                            inventoryView.Replace(item.Icon, item.RemainingUses.CurrentValue, item.Info(), i);
                        else
                            inventoryView.Remove(i);
                    }
                },
                _ => _disposables.Clear());
        }
    }
}