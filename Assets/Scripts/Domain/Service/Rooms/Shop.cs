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
using Domain.Service;
using Domain.Service.Logs;
using Domain.Model.Items;

namespace Model.Game
{
    public class Shop : Room<ShopMemento>, IDisposable
    {
        public readonly Clerk Clerk;
        private IEnumerable<Id<IItem>> _shopItems = new List<Id<IItem>>();

        public Shop(ShopMemento data, ICharacter clerk, IMapManager mapManager) : base(data.Room)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var itemMementosInRoom = itemsInRoom.Select(item => item.Id);
            foreach (var itemId in data.ItemIds)
            {
                if (!itemMementosInRoom.Contains(new Id<IItem>(itemId)))
                {
                    Debug.Log(itemsInRoom.Count());
                    Debug.Log(data.ItemIds.Count);
                    throw new Exception("ItemNotFound: I can't find an item that should be in the shop.");
                }
            }

            SetShopItems(itemsInRoom);

            Clerk = new Clerk(
                clerk,
                () => GetSalePrice(mapManager) > 0 || GetPurchasePrice(mapManager) > 0,
                mapManager => Purchase(mapManager)
            );
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
            return new ShopMemento(new RoomMemento(rect, false, false), entity, items.Select(item => item.Item.Id).ToList());
        }

        public override ShopMemento Serialize()
        {
            return new ShopMemento(new RoomMemento(Rect, hasEntered, hasEverEntered),
                Clerk.Character.Serialize().EntityData, _shopItems.Select(itemId => itemId.Value).ToList());
        }

        private IEnumerable<IItem> GetItemsInRoom(IMapManager mapManager)
        {
            return mapManager.GetItemsInArea(Rect.RectRange()).Select(item => item.Item);
        }

        private void SetShopItems(IEnumerable<IItem> items)
        {
            _shopItems = items.Select(item => item.Id);
            foreach (var item in items)
            {
                item.SetState(ItemState.ShopItem);
            }
        }
        private void RemoveMark(IEnumerable<IItem> inventoryItems)
        {
            foreach (var item in inventoryItems)
            {
                if (item.State == ItemState.ShopItem || item.State == ItemState.UsedShopItem)
                {
                    item.SetState(ItemState.None);
                }
            }
        }
        private void MarkItemsAsStolen(IMapManager mapManager)
        {
            foreach (var item in _shopItems)
            {
                mapManager.GetItemFromId(item).SetState(ItemState.Stolen);
            }
        }

        public int GetPurchasePrice(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => item.Id));
            return purchaseItems.Sum(item => mapManager.GetItemFromId(item).Price);
        }

        public int GetSalePrice(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var saleItems = itemsInRoom.Select(item => item.Id).Except(_shopItems);
            return saleItems.Sum(item => mapManager.GetItemFromId(item).Price) / 2;
        }

        public void Purchase(IMapManager mapManager)
        {
            if (mapManager.Player.Money + GetSalePrice(mapManager) >= GetPurchasePrice(mapManager))
            {
                GameLog.Add($"{mapManager.Player.Name}は{GetSalePrice(mapManager)}G受け取った");
                mapManager.Player.AddMoney(GetSalePrice(mapManager));
                GameLog.Add($"{mapManager.Player.Name}は{GetPurchasePrice(mapManager)}G支払った");
                mapManager.Player.ReduceMoney(GetPurchasePrice(mapManager));
                RemoveMark(mapManager.Player.Inventory.AllItems);
                SetShopItems(GetItemsInRoom(mapManager));
            }
            else
            {
                GameLog.Add($"{mapManager.Player.Name}は{GetPurchasePrice(mapManager) - GetSalePrice(mapManager)}G持っていなかった");
            }
        }

        protected override void UpdateTurnIfNotInside(IGameManager gameManager, IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var missingItems = _shopItems.Except(itemsInRoom.Select(item => item.Id));
            if (missingItems.Any())
            {
                GameLog.Add("どろぼう！");
                Clerk.ReducesFavorabilityTowardsThief(mapManager.Player);
                MarkItemsAsStolen(mapManager);
                CanExecute = false;
            }
        }

        protected override void EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
        }
    }
}