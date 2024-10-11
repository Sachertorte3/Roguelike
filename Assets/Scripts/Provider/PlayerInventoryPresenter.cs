#nullable enable
using Domain.Model.Character;
using Domain.Model.Item;
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
                        ReplaceItemView(inventoryView, itemChanged.NewValue, itemChanged.Index, map.Player);
                    }));
                    _disposables.Add(map.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        UpdateItemView(inventoryView, itemUpdated.Item, itemUpdated.Index, map.Player);
                    }));
                    _disposables.Add(map.Player.OnKnownItemUpdated.Subscribe(_ =>
                    {
                        for (var i = 0; i < map.Player.Inventory.MaxItemCount; i++)
                        {
                            var item = map.Player.Inventory.GetItem(i);
                            if (item != null)
                                UpdateItemView(inventoryView, item, i, map.Player);
                        }
                    }));
                    for (var i = 0; i < map.Player.Inventory.MaxItemCount; i++)
                    {
                        ReplaceItemView(inventoryView, map.Player.Inventory.GetItem(i), i, map.Player);
                    }
                },
                _ => _disposables.Clear());
        }

        public void ReplaceItemView(InventoryView inventoryView, IItem? item, int index, ICharacter player)
        {
            if (item != null)
            {
                inventoryView.Replace(
                    item.Icon,
                    item.Usable ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    player.IsKnownItem(item),
                    item.Info(player),
                    index);
            }
            else
            {
                inventoryView.Remove(index);
            }
        }

        public void UpdateItemView(InventoryView inventoryView, IItem item, int index, ICharacter player)
        {
            inventoryView.UpdateCount(
                item.Usable ? item.RemainingUses.CurrentValue : null,
                player.IsKnownItem(item),
                index);
            inventoryView.UpdateCursed(
                item.IsCursed,
                player.IsKnownItem(item),
                index);
            inventoryView.UpdateInfo(item.Info(player), index);
        }
    }
}