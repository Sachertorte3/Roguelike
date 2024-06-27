using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Items;
using UnityEngine;
using Utilities;

namespace Model.Game
{
    public class Shop : Room<ShopMemento>
    {
        private HashSet<ItemEntity> _shopItems;
        public Shop(ShopMemento data, IMapManager mapManager) : base(data.Room)
        {
            var itemsInRoom = GetItemsInRoom(mapManager);
            var itemMementosInRoom = itemsInRoom.Select(item => (item.Entity.CurrentPosition, item.Item.Info()));
            foreach (var positionAnditemInfo in data.Items.Select(item => (item.Entity.Position, new Item(item.Item).Info())))
            {
                if (!itemMementosInRoom.Contains(positionAnditemInfo))
                {
                    Debug.Log(itemsInRoom.Count);
                    Debug.Log(data.Items.Count);
                    throw new System.Exception("ItemNotFound: I can't find an item that should be in the shop.");
                }
            }
            _shopItems = itemsInRoom;
        }
        public static ShopMemento Build(RectInt rect, List<ItemEntityMemento> items)
        {
            return new ShopMemento(new(rect, false, false), items);
        }
        public override ShopMemento Serialize()
        {
            return new ShopMemento(new(Rect, hasEntered, hasEverEntered), _shopItems.Select(item => item.Serialize()).ToList());
        }
        private HashSet<ItemEntity> GetItemsInRoom(IMapManager mapManager)
        {
            return mapManager.GetItemsInArea(Rect.RectRange());
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
            Debug.Log("Exiting the Monster House.");
        }
    }
}