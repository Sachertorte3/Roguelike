#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Conditions;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Shop : Room<ShopMemento>, IShop, IDisposable
    {
        public readonly Clerk Clerk;

        private record ShopItemCache(Id<IItem> Id, int Price);

        private HashSet<ShopItemCache> _shopItems = new();
        private ReactiveProperty<bool> _isStolen = new(false);
        public ReadOnlyReactiveProperty<bool> IsStolen => _isStolen;

        public Shop(ShopMemento data, ICharacter clerk, IMap mapManager) : base(data.Room,
            mapManager.Player.CurrentPosition)
        {
            Clerk = new Clerk(
                clerk,
                (player) => CanExecute && (GetSalePrice(mapManager) > 0 || GetPurchasePrice(mapManager) > 0),
                (_, map) =>
                {
                    Purchase(map);
                    return UniTask.CompletedTask;
                }
            );

            if (data.IsStolen)
            {
                Stolen(mapManager);
                return;
            }

            _shopItems = data.Items.Select(item => new ShopItemCache(new Id<IItem>(item.Id), item.Price)).ToHashSet();
        }

        public void Dispose()
        {
            Clerk.Dispose();
        }

        ~Shop()
        {
            Dispose();
        }

        public static ShopMemento Build(RectInt rect, Id<IEntity> clerkId, List<ItemEntityMemento> items)
        {
            return new ShopMemento
            (
                new RoomMemento
                (
                    rect,
                    false,
                    false
                ),
                clerkId,
                items.Select(item => new ShopItemMemento
                (
                    item.Item.Id,
                    new Item(item.Item).Price
                )).ToList(),
                false
            );
        }

        public override ShopMemento Serialize()
        {
            return new ShopMemento
            (
                new RoomMemento
                (
                    Rect,
                    hasEntered,
                    hasEverEntered
                ),
                Clerk.Id,
                _shopItems.Select(item => new ShopItemMemento
                (
                    item.Id.ToString(),
                    item.Price
                )).ToList(),
                _isStolen.Value
            );
        }

        private IEnumerable<IItem> GetItemsInRoom(IMap mapManager)
        {
            return mapManager.GetItemsInArea(Rect.RectRange()).Select(item => item.Item);
        }

        private void SetShopItems(IEnumerable<IItem> items)
        {
            _shopItems = items.Select(item => new ShopItemCache(item.Id, item.Price)).ToHashSet();
            foreach (var item in items)
            {
                item.SetState(ItemState.ShopItem);
            }
        }

        private void RemoveMark(IMap mapManager, IEnumerable<ShopItemCache> items)
        {
            foreach (var item in items)
            {
                mapManager.GetItemFromId(item.Id)?.SetState(ItemState.None);
            }
        }

        private void MarkItemsAsStolen(IMap mapManager)
        {
            foreach (var item in _shopItems)
            {
                mapManager.GetItemFromId(item.Id)?.SetState(ItemState.Stolen);
            }
        }

        private IEnumerable<ShopItemCache> GetMissingItems(IMap mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager).Where(item => item.State == ItemState.ShopItem);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => new ShopItemCache(item.Id, item.Price)));
            return purchaseItems;
        }

        public int GetPurchasePrice(IMap mapManager)
        {
            var purchaseItems = GetMissingItems(mapManager);
            return purchaseItems.Sum(item => item.Price);
        }

        private IEnumerable<ShopItemCache> GetAddedItems(IMap mapManager)
        {
            var saleItems = GetItemsInRoom(mapManager).Where(item => item.State != ItemState.ShopItem);
            return saleItems.Select(item => new ShopItemCache(item.Id, item.Price));
        }

        public int GetSalePrice(IMap mapManager)
        {
            var saleItems = GetAddedItems(mapManager);
            return Mathf.RoundToInt(saleItems.Sum(item => item.Price) / 2f);
        }

        public void Purchase(IMap mapManager)
        {
            if (mapManager.Player.Money + GetSalePrice(mapManager) >= GetPurchasePrice(mapManager))
            {
                GameLog.Add(
                    $"{mapManager.Player.GetName(mapManager.Player)}は<color=green>{GetSalePrice(mapManager)}G</color>受け取った");
                mapManager.Player.AddMoney(GetSalePrice(mapManager));
                GameLog.Add(
                    $"{mapManager.Player.GetName(mapManager.Player)}は<color=yellow>{GetPurchasePrice(mapManager)}G</color>支払った");
                mapManager.Player.ReduceMoney(GetPurchasePrice(mapManager));
                var purchaseItems = GetMissingItems(mapManager);
                RemoveMark(mapManager, purchaseItems);
                SetShopItems(GetItemsInRoom(mapManager));
            }
            else
            {
                GameLog.Add(
                    $"{mapManager.Player.GetName(mapManager.Player)}は<color=yellow>{GetPurchasePrice(mapManager) - GetSalePrice(mapManager)}G</color>持っていなかった");
            }
        }

        public void Stolen(IMap mapManager)
        {
            GameLog.Add("<color=red>どろぼう！</color>");
            Clerk.OpposingThief(mapManager.Player);
            Clerk.Character.AddCondition(Id<IEntity>.Empty, new Clairvoyant(), new RemovalConditionData());
            MarkItemsAsStolen(mapManager);
            CanExecute = false;
            _isStolen.Value = true;
        }

        protected override async UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMap mapManager)
        {
            var missingItems = GetMissingItems(mapManager);
            if (missingItems.Any())
            {
                Stolen(mapManager);
                await UniTask.Delay(1000);
            }
        }
    }
}