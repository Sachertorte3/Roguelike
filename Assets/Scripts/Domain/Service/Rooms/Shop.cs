#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Entity;
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

        public Shop(ShopMemento data, ICharacter clerk, IMap map) : base(data.Room,
            map.Player.Character.Entity.CurrentPosition)
        {
            Clerk = new Clerk(
                clerk,
                (player) => CanExecute && (GetSalePrice(map) > 0 || GetPurchasePrice(map) > 0),
                (_, map) =>
                {
                    Purchase(map);
                    return UniTask.CompletedTask;
                }
            );

            if (data.IsStolen)
            {
                Stolen(map);
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
                Clerk.Entity.Id,
                _shopItems.Select(item => new ShopItemMemento
                (
                    item.Id.ToString(),
                    item.Price
                )).ToList(),
                _isStolen.Value
            );
        }

        private IEnumerable<IItem> GetItemsInRoom(IMap map)
        {
            return map.Items.In(Rect.RectRange()).Select(item => item.Item);
        }

        private void SetShopItems(IEnumerable<IItem> items)
        {
            _shopItems = items.Select(item => new ShopItemCache(item.Id, item.Price)).ToHashSet();
            foreach (var item in items)
            {
                item.SetState(ItemState.ShopItem);
            }
        }

        private void RemoveMark(IMap map, IEnumerable<ShopItemCache> items)
        {
            foreach (var item in items)
            {
                map.GetItemByIdFromWorldOrInventory(item.Id)?.SetState(ItemState.None);
            }
        }

        private void MarkItemsAsStolen(IMap map)
        {
            foreach (var item in _shopItems)
            {
                map.GetItemByIdFromWorldOrInventory(item.Id)?.SetState(ItemState.Stolen);
            }
        }

        private IEnumerable<ShopItemCache> GetMissingItems(IMap map)
        {
            var itemsInRoom = GetItemsInRoom(map).Where(item => item.State == ItemState.ShopItem);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => new ShopItemCache(item.Id, item.Price)));
            return purchaseItems;
        }

        public int GetPurchasePrice(IMap map)
        {
            var purchaseItems = GetMissingItems(map);
            if (map.Player.Character.Status.IsFlagStat(FlagStatType.Haggle))
            {
                return Mathf.RoundToInt(purchaseItems.Sum(item => item.Price) / 2f);
            }
            return purchaseItems.Sum(item => item.Price);
        }

        private IEnumerable<ShopItemCache> GetAddedItems(IMap map)
        {
            var saleItems = GetItemsInRoom(map).Where(item => item.State != ItemState.ShopItem);
            return saleItems.Select(item => new ShopItemCache(item.Id, item.Price));
        }

        public int GetSalePrice(IMap map)
        {
            var saleItems = GetAddedItems(map);
            return Mathf.RoundToInt(saleItems.Sum(item => item.Price) / 2f);
        }

        public void Purchase(IMap map)
        {
            if (map.Player.Character.Money + GetSalePrice(map) >= GetPurchasePrice(map))
            {
                GameLog.Add(
                    $"{map.Player.Character.GetName(map.Player)}は<color=green>{GetSalePrice(map)}G</color>受け取った");
                map.Player.Character.AddMoney(GetSalePrice(map));
                GameLog.Add(
                    $"{map.Player.Character.GetName(map.Player)}は<color=yellow>{GetPurchasePrice(map)}G</color>支払った");
                map.Player.Character.ReduceMoney(GetPurchasePrice(map));
                var purchaseItems = GetMissingItems(map);
                RemoveMark(map, purchaseItems);
                SetShopItems(GetItemsInRoom(map));
            }
            else
            {
                GameLog.Add(
                    $"{map.Player.Character.GetName(map.Player)}は<color=yellow>{GetPurchasePrice(map) - GetSalePrice(map)}G</color>持っていなかった");
            }
        }

        public void Stolen(IMap map)
        {
            GameLog.Add("<color=red>どろぼう！</color>");
            Clerk.OpposingThief(map.Player.Character);
            Clerk.Character.AddCondition(Id<IEntity>.Empty, new Clairvoyant(), new RemovalConditionData());
            MarkItemsAsStolen(map);
            CanExecute = false;
            _isStolen.Value = true;
        }

        protected override async UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMap map)
        {
            var missingItems = GetMissingItems(map);
            if (missingItems.Any())
            {
                Stolen(map);
                await UniTask.Delay(1000);
            }
        }
    }
}