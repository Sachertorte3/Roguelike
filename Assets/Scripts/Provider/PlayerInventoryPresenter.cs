#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
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
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
                {
                    map.Player.Character.Inventory.OnItemChanged.Subscribe(itemChanged =>
                    {
                        ReplaceItemView(inventoryView, itemChanged.NewValue, itemChanged.Index, map.Player,
                            map.ItemPlaceholders);
                    }).AddTo(_disposables);
                    gameManager.Turn.Subscribe(position => { UpdateGroundItemView(inventoryView, map); })
                        .AddTo(_disposables);
                    map.Player.Character.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        UpdateItemView(inventoryView, itemUpdated.Item, itemUpdated.Index, map.Player,
                            map.ItemPlaceholders);
                    }).AddTo(_disposables);
                    map.Player.Character.OnKnownItemUpdated.Subscribe(_ => { UpdateAllItemViews(inventoryView, map); })
                        .AddTo(_disposables);
                    map.ItemPlaceholders.OnItemRenamed.Subscribe(_ => { UpdateAllItemViews(inventoryView, map); })
                        .AddTo(_disposables);
                    for (var i = 0; i < map.Player.Character.Inventory.MaxItemCount; i++)
                    {
                        ReplaceItemView(inventoryView, map.Player.Character.Inventory.GetItem(i), i, map.Player,
                            map.ItemPlaceholders);
                    }
                },
                _ => _disposables.Clear());
        }

        private void ReplaceItemView(InventoryView inventoryView, IItem? item, int index, IPlayer player,
            ItemPlaceholders itemPlaceholders)
        {
            if (item != null)
            {
                inventoryView.Replace(
                    item.Icon,
                    item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    player.Character.IsKnownItem(item),
                    item.IsCurseIdentified,
                    item.Info(player, itemPlaceholders),
                    index);
            }
            else
            {
                inventoryView.Remove(index);
            }
        }

        private void UpdateAllItemViews(InventoryView inventoryView, IMap map)
        {
            for (var i = 0; i < map.Player.Character.Inventory.MaxItemCount; i++)
            {
                var item = map.Player.Character.Inventory.GetItem(i);
                if (item != null)
                    UpdateItemView(inventoryView, item, i, map.Player, map.ItemPlaceholders);
            }

            UpdateGroundItemView(inventoryView, map);
        }

        private void UpdateGroundItemView(InventoryView inventoryView, IMap map)
        {
            var item = map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault();
            if (item != null)
                ReplaceItemView(inventoryView, item.Item, map.Player.Character.Inventory.MaxItemCount, map.Player,
                    map.ItemPlaceholders);
            else
                inventoryView.SetGround();
        }

        private void UpdateItemView(InventoryView inventoryView, IItem item, int index, IPlayer player,
            ItemPlaceholders itemPlaceholders)
        {
            ReplaceItemView(inventoryView, item, index, player, itemPlaceholders);
        }
    }
}