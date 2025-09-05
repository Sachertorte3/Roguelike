#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Setting;
using Game;
using ObservableCollections;
using R3;
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
        public PlayerInventoryPresenter(GameManager gameManager, World world, InventoryView inventoryView)
        {
            inventoryView.Initialize();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
                {
                    var inventory = map.Player.Character.Inventory;
                    Observable.Merge<(IItem? Item, int Index)>(
                        inventory.OnItemChanged.Select(itemChanged => ((IItem?)itemChanged.NewItem, itemChanged.Index)),
                        inventory.OnItemUpdated.Select(itemUpdated => ((IItem?)itemUpdated.Item, itemUpdated.Index))
                    ).Subscribe(data =>
                    {
                        ReplaceItemView(inventoryView, data.Item, new InventoryViewIndex(data.Index),
                            map.Player, map.ItemPlaceholders);
                    }).AddTo(_disposables);

                    gameManager.OnTurnChanged.Subscribe(_ =>
                    {
                        UpdateGroundItemView(inventoryView, map);
                    }).AddTo(_disposables);

                    Observable.Merge(
                        map.Player.Character.KnownItemNames.ObserveChanged().AsUnitObservable(),
                        map.ItemPlaceholders.OnItemRenamed
                    ).Subscribe(_ =>
                    {
                        UpdateAllItemViews(inventoryView, map);
                    }).AddTo(_disposables);

                    foreach (var (item, index) in inventory.AllItemsWithIndex)
                    {
                        ReplaceItemView(inventoryView, item, new InventoryViewIndex(index),
                            map.Player, map.ItemPlaceholders);
                    }
                },
                _ => _disposables.Clear());
        }

        private IItem? GetGroundItem(IMap map)
        {
            return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
        }

        private void ReplaceItemView(InventoryView inventoryView, IItem? item,
            InventoryViewIndex index, IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (item != null)
            {
                inventoryView.Replace(
                    index,
                    new ItemViewData(
                        item.Icon,
                        item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                        item.IsCursed,
                        item.IsShiny,
                        player.Character.IsKnownItem(item),
                        item.IsCurseIdentified,
                        item.ItemStorage.MapOr(0, storage => storage.Capacity),
                        item.Info(player, itemPlaceholders)
                    )
                );
                if (item.ItemStorage.IsSome)
                {
                    for (var subIndex = 0; subIndex < item.ItemStorage.Value.Capacity; subIndex++)
                    {
                        var subItem = item.ItemStorage.Value.GetItem(subIndex);
                        ReplaceItemView(inventoryView, subItem, new InventoryViewIndex(index.Index, subIndex), player, itemPlaceholders);
                    }
                }
            }
            else
            {
                inventoryView.Remove(index);
            }
        }

        private void UpdateAllItemViews(InventoryView inventoryView, IMap map)
        {
            for (var i = 0; i < InventoryView.MainStorageSize; i++)
            {
                var item = map.Player.Character.Inventory.GetItem(i);
                ReplaceItemView(inventoryView, item, new InventoryViewIndex(i), map.Player, map.ItemPlaceholders);
            }
            UpdateGroundItemView(inventoryView, map);
        }

        private void UpdateGroundItemView(InventoryView inventoryView, IMap map)
        {
            var item = GetGroundItem(map);
            if (item != null)
                ReplaceItemView(inventoryView, item, InventoryViewIndex.GroundItem,
                    map.Player, map.ItemPlaceholders);
            else
                inventoryView.Remove(InventoryViewIndex.GroundItem);
        }
    }
}