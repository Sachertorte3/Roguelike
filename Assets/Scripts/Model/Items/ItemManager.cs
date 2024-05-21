#nullable enable
using Model.Characters;
using ObservableCollections;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Model.Items
{
    public sealed class ItemManager
    {
        private HashSet<Vector2Int> _allItemPositions = new();
        private readonly ItemFactory _factory = new();
        private readonly ObservableList<ItemEntity> _items = new();
        public ItemEntityEvents ItemEntityEvents = new();

        [Inject]
        public ItemManager(Character player)
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            ItemEntityEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllItemPosition();
                positionChanged.Item.SetVisiblity(player.Area.Get().Contains(positionChanged.Position));
            });
            ItemEntityEvents.OnDisabled.Subscribe(dead => _items.Remove(dead.Item));
        }

        internal IObservableCollection<ItemEntity> Items => _items;

        public void AddItem(ItemEntity item)
        {
            _items.Add(item);
            ItemEntityEvents.Add(item);
        }

        public ItemEntity SpawnItem(Item item, Vector2Int spawnPosition)
        {
            var itemEntity = _factory.CreateItem(spawnPosition, item);
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
    }
}