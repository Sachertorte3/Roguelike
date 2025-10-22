#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Game;
using ObservableCollections;
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
            inventoryView.Initialize();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    var map = mapChanged.Map;
                    var inventory = map.Player.Character.Inventory;
                    inventoryView.Reset(
                        inventory.AllItems.Select(
                            item => new ItemViewData(
                                item.GetName(map.Player, map.ItemPlaceholders),
                                item.Icon,
                                true,
                                item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                                item.IsCursed,
                                item.IsShiny,
                                map.Player.Character.IsKnownItem(item),
                                item.IsCurseIdentified,
                                item.Info(map.Player, map.ItemPlaceholders)
                            )
                        ).ToList(),
                        mapChanged.IsNewWorld);

                    inventory.OnItemInserted.Subscribe(inserted =>
                    {
                        InsertItemView(inventoryView, inserted.NewItem, inventory.CanRemoveItem, inserted.Index,
                            map.Player, map.ItemPlaceholders);
                    }).AddTo(_disposables);

                    inventory.OnItemRemoved.Subscribe(removed =>
                    {
                        RemoveItemView(inventoryView, removed.Index);
                    }).AddTo(_disposables);

                    inventory.OnItemReplaced.Subscribe(replaced =>
                    {
                        ReplaceItemView(inventoryView, replaced.NewItem, inventory.CanRemoveItem, replaced.Index,
                            map.Player, map.ItemPlaceholders);
                    }).AddTo(_disposables);

                    inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        var index = inventory.GetItemIndex(itemUpdated.Item);
                        if (index == null)
                            return;
                        ReplaceItemView(inventoryView, itemUpdated.Item, inventory.CanRemoveItem, index.Value,
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
                        ReplaceAllItemViews(inventoryView, map);
                    }).AddTo(_disposables);

                    ReplaceAllItemViews(inventoryView, map);
                },
                _ => _disposables.Clear()
            );
        }

        private void ReplaceAllItemViews(InventoryView inventoryView, IMap map)
        {
            var inventory = map.Player.Character.Inventory;
            foreach (var (item, index) in inventory.AllItemsWithIndex)
            {
                ReplaceItemView(inventoryView, item, inventory.CanRemoveItem, index, map.Player, map.ItemPlaceholders);
            }
            UpdateGroundItemView(inventoryView, map);
        }

        private void InsertItemView(InventoryView inventoryView, IItem item, bool canSelect,
            int index, IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            inventoryView.Insert(
                index,
                new ItemViewData(
                    item.GetName(player, itemPlaceholders),
                    item.Icon,
                    canSelect,
                    item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    player.Character.IsKnownItem(item),
                    item.IsCurseIdentified,
                    item.Info(player, itemPlaceholders)
                )
            );
        }

        private void RemoveItemView(InventoryView inventoryView, int index)
        {
            inventoryView.Remove(index);
        }

        private void ReplaceItemView(InventoryView inventoryView, IItem item, bool canSelect,
            int index, IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            inventoryView.Replace(
                index,
                new ItemViewData(
                    item.GetName(player, itemPlaceholders),
                    item.Icon,
                    canSelect,
                    item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    player.Character.IsKnownItem(item),
                    item.IsCurseIdentified,
                    item.Info(player, itemPlaceholders)
                )
            );
        }

        private IItem? GetGroundItem(IMap map)
        {
            return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
        }

        private void UpdateGroundItemView(InventoryView inventoryView, IMap map)
        {
            var item = GetGroundItem(map);
            if (item == null)
                inventoryView.UpdateGroundItem(null);
            else
                inventoryView.UpdateGroundItem(new ItemViewData(
                    item.GetName(map.Player, map.ItemPlaceholders),
                    item.Icon,
                    true,
                    item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                    item.IsCursed,
                    item.IsShiny,
                    map.Player.Character.IsKnownItem(item),
                    item.IsCurseIdentified,
                    item.Info(map.Player, map.ItemPlaceholders)
                ));
        }
    }
}