#nullable enable
using Game;
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
                        {
                            var newItem = itemChanged.NewValue;
                            inventoryView.Replace(
                                newItem.Icon,
                                newItem.Usable ? newItem.RemainingUses.CurrentValue : null,
                                newItem.IsCursed,
                                newItem.IsShiny,
                                newItem.Info(),
                                itemChanged.Index);
                        }
                        else
                            inventoryView.Remove(itemChanged.Index);
                    }));
                    _disposables.Add(map.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        inventoryView.UpdateCount(
                            itemUpdated.Item.Usable ? itemUpdated.Item.RemainingUses.CurrentValue : null,
                            itemUpdated.Index);
                        inventoryView.UpdateCursed(itemUpdated.Item.IsCursed, itemUpdated.Index);
                        inventoryView.UpdateInfo(itemUpdated.Item.Info(), itemUpdated.Index);
                    }));
                    for (var i = 0; i < map.Player.Inventory.MaxItemCount; i++)
                    {
                        var item = map.Player.Inventory.GetItem(i);
                        if (item != null)
                            inventoryView.Replace(
                                item.Icon,
                                item.Usable ? item.RemainingUses.CurrentValue : null,
                                item.IsCursed,
                                item.IsShiny,
                                item.Info(),
                                i);
                        else
                            inventoryView.Remove(i);
                    }
                },
                _ => _disposables.Clear());
        }
    }
}