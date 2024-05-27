#nullable enable
using Model;
using Model.Game;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class PlayerInventoryPresenter
    {
        [Inject]
        public PlayerInventoryPresenter(World world, InventoryView inventoryView)
        {
            world.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
            {
                if (itemChanged.NewValue != null)
                    inventoryView.Replace(itemChanged.NewValue.Icon, itemChanged.NewValue.RemainingUses.CurrentValue,
                        itemChanged.NewValue.Info, itemChanged.Index);
                else
                    inventoryView.Remove(itemChanged.Index);
            });
            world.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
            {
                inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
            });
        }
    }
}