#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
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
        public PlayerInventoryPresenter(GameManager gameManager, World world, InventoryView inventoryView)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    _disposables.Add(map.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
                    {
                        ReplaceItemView(inventoryView, itemChanged.NewValue, itemChanged.Index, map.Player, map.ItemPlaceholders);
                    }));
                    _disposables.Add(gameManager.Turn.Subscribe(position =>
                    {
                        var item = map.Items.At(map.Player.CurrentPosition).FirstOrDefault();
                        if (item != null)
                            inventoryView.UpdateInfo(item.Item.Info(map.Player, map.ItemPlaceholders), map.Player.Inventory.MaxItemCount);
                        else
                            inventoryView.UpdateInfo("", map.Player.Inventory.MaxItemCount);
                    }));
                    _disposables.Add(map.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        UpdateItemView(inventoryView, itemUpdated.Item, itemUpdated.Index, map.Player, map.ItemPlaceholders);
                    }));
                    _disposables.Add(map.Player.OnKnownItemUpdated.Subscribe(_ =>
                    {
                        UpdateAllItemViews(inventoryView, map.Player, map.ItemPlaceholders);
                    }));
                    _disposables.Add(map.ItemPlaceholders.OnItemRenamed.Subscribe(_ =>
                    {
                        UpdateAllItemViews(inventoryView, map.Player, map.ItemPlaceholders);
                    }));
                    for (var i = 0; i < map.Player.Inventory.MaxItemCount; i++)
                    {
                        ReplaceItemView(inventoryView, map.Player.Inventory.GetItem(i), i, map.Player, map.ItemPlaceholders);
                    }
                },
                _ => _disposables.Clear());
        }

        public void ReplaceItemView(InventoryView inventoryView, IItem? item, int index, ICharacter player, ItemPlaceholders itemPlaceholders)
        {
            if (item != null)
            {
                inventoryView.Replace(
                    item.Icon,
                    item.Usable ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    player.IsKnownItem(item),
                    player.IsKnownItem(item) || item.IsCurseIdentified,
                    item.Info(player, itemPlaceholders),
                    index);
            }
            else
            {
                inventoryView.Remove(index);
            }
        }

        public void UpdateAllItemViews(InventoryView inventoryView, ICharacter player, ItemPlaceholders itemPlaceholders)
        {
            for (var i = 0; i < player.Inventory.MaxItemCount; i++)
            {
                var item = player.Inventory.GetItem(i);
                if (item != null)
                    UpdateItemView(inventoryView, item, i, player, itemPlaceholders);
            }
        }

        public void UpdateItemView(InventoryView inventoryView, IItem item, int index, ICharacter player, ItemPlaceholders itemPlaceholders)
        {
            inventoryView.UpdateCount(
                item.Usable ? item.RemainingUses.CurrentValue : null,
                player.IsKnownItem(item),
                index);
            inventoryView.UpdateCursed(
                item.IsCursed,
                player.IsKnownItem(item) || item.IsCurseIdentified,
                index);
            inventoryView.UpdateInfo(item.Info(player, itemPlaceholders), index);
        }
    }
}