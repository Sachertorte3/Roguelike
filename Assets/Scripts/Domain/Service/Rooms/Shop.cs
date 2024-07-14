#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Rooms;
using UnityEngine;
using Utilities;
using R3;
using Domain.Service.Logs;
using Domain.Model.Items;
using Unity.Logging;
using Domain.Model;
using Domain.Service.Characters.Conditions;
using Domain.Model.Condition;

namespace Model.Game
{
    public class Shop : Room<ShopMemento>, IShop, IDisposable
    {
        public readonly Clerk Clerk;
        private record ShopItemCache(Id<IItem> Id, int Price);
        private HashSet<ShopItemCache> _shopItems = new HashSet<ShopItemCache>();
        private ReactiveProperty<bool> _isStolen = new ReactiveProperty<bool>(false);
        public ReadOnlyReactiveProperty<bool> IsStolen => _isStolen;

        public Shop(ShopMemento data, ICharacter clerk, IMapManager mapManager) : base(data.Room)
        {
            Clerk = new Clerk(
                clerk,
                () => CanExecute && GetSalePrice(mapManager) > 0 || GetPurchasePrice(mapManager) > 0,
                mapManager => Purchase(mapManager)
            );

            if (data.IsStolen)
            {
                Stolen(mapManager);
                return;
            }

            var itemsInRoom = GetItemsInRoom(mapManager);
            var itemMementosInRoom = itemsInRoom.Select(item => item.Id);
            foreach (var item in data.Items)
            {
                if (!itemMementosInRoom.Contains(new Id<IItem>(item.Id)))
                {
                    Debug.Log(itemsInRoom.Count());
                    Debug.Log(data.Items.Count);
                    throw new Exception("ItemNotFound: I can't find an item that should be in the shop.");
                }
            }

            SetShopItems(itemsInRoom);
        }

        public void Dispose()
        {
            Clerk.Dispose();
        }

        ~Shop()
        {
            Dispose();
        }

        public static ShopMemento Build(RectInt rect, EntityMemento entity, List<ItemEntityMemento> items)
        {
            return new ShopMemento(
                new RoomMemento(rect, false, false),
                entity,
                items.Select(item => new ShopItemMemento(item.Item.Id, item.Item.Price)).ToList(),
                false
            );
        }

        public override ShopMemento Serialize()
        {
            return new ShopMemento(
                new RoomMemento(Rect, hasEntered, hasEverEntered),
                Clerk.Character.Serialize().EntityData,
                _shopItems.Select(item => new ShopItemMemento(item.Id.Value, item.Price)).ToList(),
                _isStolen.Value
            );
        }

        private IEnumerable<IItem> GetItemsInRoom(IMapManager mapManager)
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
        private void RemoveMark(IMapManager mapManager, IEnumerable<ShopItemCache> items)
        {
            foreach (var item in items)
            {
                mapManager.GetItemFromId(item.Id)?.SetState(ItemState.None);
            }
        }
        private void MarkItemsAsStolen(IMapManager mapManager)
        {
            foreach (var item in _shopItems)
            {
                mapManager.GetItemFromId(item.Id)?.SetState(ItemState.Stolen);
            }
        }

        private IEnumerable<ShopItemCache> GetMissingItems(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager).Where(item => item.State == ItemState.ShopItem);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => new ShopItemCache(item.Id, item.Price)));
            return purchaseItems;
        }
        public int GetPurchasePrice(IMapManager mapManager)
        {
            var purchaseItems = GetMissingItems(mapManager);
            return purchaseItems.Sum(item => item.Price);
        }

        private IEnumerable<ShopItemCache> GetAddedItems(IMapManager mapManager)
        {
            var saleItems = GetItemsInRoom(mapManager).Where(item => item.State != ItemState.ShopItem);
            return saleItems.Select(item => new ShopItemCache(item.Id, item.Price));
        }
        public int GetSalePrice(IMapManager mapManager)
        {
            var saleItems = GetAddedItems(mapManager);
            return Mathf.RoundToInt(saleItems.Sum(item => item.Price) / 2f);
        }

        public void Purchase(IMapManager mapManager)
        {
            if (mapManager.Player.Money + GetSalePrice(mapManager) >= GetPurchasePrice(mapManager))
            {
                GameLog.Add($"{mapManager.Player.GetName(mapManager.Player)}は<color=green>{GetSalePrice(mapManager)}G</color>受け取った");
                mapManager.Player.AddMoney(GetSalePrice(mapManager));
                GameLog.Add($"{mapManager.Player.GetName(mapManager.Player)}は<color=yellow>{GetPurchasePrice(mapManager)}G</color>支払った");
                mapManager.Player.ReduceMoney(GetPurchasePrice(mapManager));
                var purchaseItems = GetMissingItems(mapManager);
                RemoveMark(mapManager, purchaseItems);
                SetShopItems(GetItemsInRoom(mapManager));
            }
            else
            {
                GameLog.Add($"{mapManager.Player.GetName(mapManager.Player)}は<color=yellow>{GetPurchasePrice(mapManager) - GetSalePrice(mapManager)}G</color>持っていなかった");
            }
        }

        public void Stolen(IMapManager mapManager)
        {
            GameLog.Add("<color=red>どろぼう！</color>");
            Clerk.ReducesFavorabilityTowardsThief(mapManager.Player);
            Clerk.Character.AddCondition(new Clairvoyant(), new RemovalConditionData());
            MarkItemsAsStolen(mapManager);
            CanExecute = false;
            _isStolen.Value = true;
        }

        protected override void UpdateTurnIfNotInside(IGameManager gameManager, IMapManager mapManager)
        {
            var missingItems = GetMissingItems(mapManager);
            if (missingItems.Any())
            {
                Stolen(mapManager);
            }
        }

        protected override void UpdateTurnIfInside(IGameManager gameManager, IMapManager mapManager)
        {
            foreach (var item in _shopItems)
            {
                Log.Debug(item.ToString());
            }
        }
    }
}