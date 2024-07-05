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
        private IDisposable _disposable;
        private IEnumerable<IItem> _shopItems;

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

            _shopItems = itemsInRoom;

            Clerk = new Clerk(clerk);

            _disposable = Clerk.OnEventDone.Subscribe(_ =>
            {
                if (mapManager.Player.Money >= GetPurchasePrice(mapManager))
                {
                    GameLog.Add($"{mapManager.Player.Name}は{GetPurchasePrice(mapManager)}G支払った");
                    mapManager.Player.ReduceMoney(GetPurchasePrice(mapManager));
                    Purchase(mapManager);
                }
                else
                {
                    GameLog.Add($"{mapManager.Player.Name}は{GetPurchasePrice(mapManager)}G持っていなかった");
                }
            });
        }

        public void Dispose()
        {
            Clerk.Dispose();
            _disposable.Dispose();
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
                Clerk.Character.Serialize().EntityData, _shopItems.Select(item => item.Id.Value).ToList());
        }

        private IEnumerable<IItem> GetItemsInRoom(IMapManager mapManager)
        {
            return mapManager.GetItemsInArea(Rect.RectRange()).Select(item => item.Item);
        }

        public int GetPurchasePrice(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var purchaseItems = _shopItems.Except(itemsInRoom);
            return purchaseItems.Sum(item => item.Price);
        }

        public void Purchase(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var remainShopItems = _shopItems.Intersect(itemsInRoom);
            _shopItems = remainShopItems.ToHashSet();
        }

        protected override void UpdateTurnIfNotInside(IGameManager gameManager, IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var missingItems = _shopItems.Except(itemsInRoom);
            if (missingItems.Any())
            {
                GameLog.Add("どろぼう！");
                Clerk.ReducesFavorabilityTowardsThief(mapManager.Player);
            }
        }

        protected override void EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
        }
    }
}