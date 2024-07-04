using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Rooms;
using UnityEngine;
using Utilities;
using R3;
using Unity.Logging;
using Domain.Service;

namespace Model.Game
{
    public class Shop : Room<ShopMemento>, IDisposable
    {
        public readonly Clerk Clerk;
        private IDisposable _disposable;
        private HashSet<IItemEntity> _shopItems;

        public Shop(ShopMemento data, ICharacter clerk, IMapManager mapManager) : base(data.Room)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var itemMementosInRoom = itemsInRoom.Select(item => (item.CurrentPosition, item.Item.Info()));
            foreach (var positionAnditemInfo in data.Items.Select(item =>
                         (item.Entity.Position, new Item(item.Item).Info())))
            {
                if (!itemMementosInRoom.Contains(positionAnditemInfo))
                {
                    Debug.Log(itemsInRoom.Count);
                    Debug.Log(data.Items.Count);
                    throw new Exception("ItemNotFound: I can't find an item that should be in the shop.");
                }
            }

            _shopItems = itemsInRoom;

            Clerk = new Clerk(clerk);

            _disposable = Clerk.OnEventDone.Subscribe(_ =>
            {
                if (mapManager.Player.Money >= GetPurchasePrice(mapManager))
                {
                    Log.Debug("purchase");
                    mapManager.Player.ReduceMoney(GetPurchasePrice(mapManager));
                    Purchase(mapManager);
                }
                else
                {
                    Log.Debug("You don't have enough money");
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
            return new ShopMemento(new RoomMemento(rect, false, false), entity, items);
        }

        public override ShopMemento Serialize()
        {
            return new ShopMemento(new RoomMemento(Rect, hasEntered, hasEverEntered),
                Clerk.Character.Serialize().EntityData, _shopItems.Select(item => item.Serialize()).ToList());
        }

        private HashSet<IItemEntity> GetItemsInRoom(IMapManager mapManager)
        {
            return mapManager.GetItemsInArea(Rect.RectRange());
        }

        public int GetPurchasePrice(IMapManager mapManager)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var purchaseItems = _shopItems.Except(itemsInRoom);
            return purchaseItems.Sum(item => item.Item.Price);
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
                Debug.Log("Thief detected in the shop!");
            }
        }

        protected override void EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
        }
    }
}