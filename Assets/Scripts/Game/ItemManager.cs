#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Items;
using ObservableCollections;
using R3;
using UnityEngine;
using VContainer;

namespace Model.Game
{
    public sealed class ItemManager : IDisposable
    {
        private readonly ItemFactory _factory = new();
        private readonly ObservableList<IItemEntity> _items = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        public ItemEntityEvents ItemEntityEvents = new();

        [Inject]
        public ItemManager()
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            ItemEntityEvents.OnPositionChanged.Subscribe(_ => SetAllItemPosition());
            ItemEntityEvents.OnDisabled.Subscribe(dead => _items.Remove(dead.Item));
        }

        public IObservableCollection<IItemEntity> Items => _items;

        public void Dispose()
        {
            _items.ForEach(item => item.Dispose());
            ItemEntityEvents.Dispose();
        }

        ~ItemManager()
        {
            Dispose();
        }

        public void AddItem(IItemEntity item)
        {
            _items.Add(item);
            ItemEntityEvents.Add(item);
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int spawnPosition)
        {
            var itemEntity = _factory.CreateItem(ItemFactory.Build(spawnPosition, item.Serialize()));
            AddItem(itemEntity);
            return itemEntity;
        }

        public IItemEntity SpawnItem(ItemEntityMemento item)
        {
            var itemEntity = _factory.CreateItem(item);
            AddItem(itemEntity);
            return itemEntity;
        }

        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Select(item => item.CurrentPosition).ToHashSet();
        }

        public IItemEntity? GetItemAt(Vector2Int position)
        {
            return _items.FirstOrDefault(item => item.CurrentPosition == position);
        }

        public IItemEntity? TryPickUp(Vector2Int position, bool pickUpShopItem = false)
        {
            var item = GetItemAt(position);
            if (item == null)
                return null;

            if (pickUpShopItem || item.Item.State != ItemState.ShopItem)
            {
                _items.Remove(item);
                ItemEntityEvents.Remove(item);
                return item;
            }
            else
                return null;
        }
    }
}