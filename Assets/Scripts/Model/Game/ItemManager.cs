#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Data.Map;
using Model.Domain.Items;
using ObservableCollections;
using R3;
using UnityEngine;
using VContainer;

namespace Model.Game
{
    public sealed class ItemManager : IDisposable
    {
        private readonly ItemFactory _factory = new();
        private readonly ObservableList<ItemEntity> _items = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        public ItemEntityEvents ItemEntityEvents = new();

        [Inject]
        public ItemManager()
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            ItemEntityEvents.OnPositionChanged.Subscribe(positionChanged => { SetAllItemPosition(); });
            ItemEntityEvents.OnDisabled.Subscribe(dead => _items.Remove(dead.Item));
        }

        public IObservableCollection<ItemEntity> Items => _items;

        public void Dispose()
        {
            _items.ForEach(item => item.Dispose());
            ItemEntityEvents.Dispose();
        }

        ~ItemManager()
        {
            Dispose();
        }

        public void AddItem(ItemEntity item)
        {
            _items.Add(item);
            ItemEntityEvents.Add(item);
        }

        public ItemEntity SpawnItem(Item item, Vector2Int spawnPosition)
        {
            var itemEntity = _factory.CreateItem(ItemEntity.Build(spawnPosition, item));
            AddItem(itemEntity);
            return itemEntity;
        }

        public ItemEntity SpawnItem(ItemEntityMemento item)
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

        public ItemEntity? TryPickUp(Vector2Int position)
        {
            if (GetAllItemPositions().Contains(position))
            {
                var item = _items.First(item => item.CurrentPosition == position);
                _items.Remove(item);
                return item;
            }

            return null;
        }
    }
}