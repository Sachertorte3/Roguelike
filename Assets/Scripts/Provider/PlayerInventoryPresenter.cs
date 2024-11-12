#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Game;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class PlayerInventoryPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public PlayerInventoryPresenter(GameManager gameManager, World world, InventoryView inventoryView,
            SubStorageView subStorageView)
        {
            inventoryView.Initialize(subStorageView);
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
                {
                    inventoryView.Select(0);
                    map.Player.Character.Inventory.OnItemChanged.Subscribe(itemChanged =>
                    {
                        ReplaceItemView(inventoryView, subStorageView, itemChanged.NewValue, itemChanged.Index,
                            map.Player, map.ItemPlaceholders);
                    }).AddTo(_disposables);
                    gameManager.Turn.Subscribe(position => { UpdateGroundItemView(inventoryView, subStorageView, map); })
                        .AddTo(_disposables);
                    map.Player.Character.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        UpdateItemView(inventoryView, subStorageView, itemUpdated.Item, itemUpdated.Index,
                            map.Player, map.ItemPlaceholders);
                    }).AddTo(_disposables);
                    map.Player.Character.OnKnownItemUpdated.Subscribe(_ => { UpdateAllItemViews(inventoryView, subStorageView, map); })
                        .AddTo(_disposables);
                    map.ItemPlaceholders.OnItemRenamed.Subscribe(_ => { UpdateAllItemViews(inventoryView, subStorageView, map); })
                        .AddTo(_disposables);
                    inventoryView.OnMainFocusChanged.Subscribe(index =>
                    {
                        IItem? item = null;
                        if (index.isEmpty)
                        {
                            item = null;
                        }
                        else if (index.isGroundItem)
                        {
                            item = GetGroundItem(map);
                        }
                        else
                        {
                            item = map.Player.Character.Inventory.GetItem(index.index);
                        }
                        UpdateSubStorageView(inventoryView, subStorageView, item, index.index, map.Player,
                            map.ItemPlaceholders);
                    }).AddTo(_disposables);
                    for (var i = 0; i < map.Player.Character.Inventory.Capacity; i++)
                    {
                        ReplaceItemView(inventoryView, subStorageView, map.Player.Character.Inventory.GetItem(i), i,
                            map.Player, map.ItemPlaceholders);
                    }
                },
                _ => _disposables.Clear());
        }

        private IItem? GetGroundItem(IMap map)
        {
            return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
        }

        private void ReplaceItemView(InventoryView inventoryView, SubStorageView subStorageView, IItem? item,
            int index, IPlayer player, ItemPlaceholders itemPlaceholders)
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
            if (index == inventoryView.CurrentFocus.index)
                UpdateSubStorageView(inventoryView, subStorageView, item, index, player, itemPlaceholders);
        }

        private void UpdateSubStorageView(InventoryView inventoryView, SubStorageView subStorageView, IItem? item,
            int index, IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            Log.Info($"UpdateSubStorageView");
            var focus = inventoryView.CurrentFocus;
            if (item != null && item.ItemStorage.IsSome)
            {
                subStorageView.SetCapacity(inventoryView.Get(index), index, item.ItemStorage.Value.Capacity);
                inventoryView.SetNavigationWithSubStorage(subStorageView, index);
                for (var i = 0; i < item.ItemStorage.Value.Capacity; i++)
                {
                    var subStorageItem = item.ItemStorage.Value.GetItem(i);
                    if (subStorageItem != null)
                        subStorageView.Replace(
                            subStorageItem.Icon,
                            subStorageItem.RemainingUses.CurrentValue,
                            subStorageItem.IsCursed,
                            subStorageItem.IsShiny,
                            player.Character.IsKnownItem(subStorageItem),
                            subStorageItem.IsCurseIdentified,
                            subStorageItem.Info(player, itemPlaceholders),
                            i);
                    else
                        subStorageView.Remove(i);
                }
            }
            else
            {
                inventoryView.SetNavigation(index);
                subStorageView.Clear();
            }
            if (focus.subIndex >= 0 && focus.subIndex < subStorageView.Capacity)
                subStorageView.Select(focus.subIndex);
            else
                inventoryView.Select(focus.index);
        }

        private void UpdateAllItemViews(InventoryView inventoryView, SubStorageView subStorageView, IMap map)
        {
            for (var i = 0; i < map.Player.Character.Inventory.Capacity; i++)
            {
                var item = map.Player.Character.Inventory.GetItem(i);
                if (item != null)
                    UpdateItemView(inventoryView, subStorageView, item, i, map.Player, map.ItemPlaceholders);
            }

            UpdateGroundItemView(inventoryView, subStorageView, map);
        }

        private void UpdateGroundItemView(InventoryView inventoryView, SubStorageView subStorageView, IMap map)
        {
            var item = GetGroundItem(map);
            if (item != null)
                ReplaceItemView(inventoryView, subStorageView, item, map.Player.Character.Inventory.Capacity,
                    map.Player, map.ItemPlaceholders);
            else
                inventoryView.SetGround();
        }

        private void UpdateItemView(InventoryView inventoryView, SubStorageView subStorageView, IItem item, int index,
            IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            ReplaceItemView(inventoryView, subStorageView, item, index, player, itemPlaceholders);
        }
    }
}