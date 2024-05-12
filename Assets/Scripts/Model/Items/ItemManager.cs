#nullable enable
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Scripts.Data;
using Scripts.Data.Area;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Model.Items
{
    public sealed class ItemManager
    {
        private ObservableList<ItemEntity> _items = new();
        public ReadOnlyCollection<ItemEntity> Items => new(_items);
        public Observable<ItemEntity> OnItemAdded => _items.ObserveAdd().Select(item => item.Value);
        public Observable<ItemEntity> OnItemRemoved => _items.ObserveRemove().Select(item => item.Value);
        private ItemFactory _factory = new();
        public ItemEntityEvents EntityEvents = new();
        public ItemManager()
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            EntityEvents.OnPositionChanged.Subscribe(_ => SetAllItemPosition());
        }
        public void AddItem(ItemEntity item)
        {
            _items.Add(item);
            EntityEvents.Add(item);
        }
        public void SpawnItem(Item item, Vector2Int spawnPosition)
        {
            AddItem(_factory.CreateItem(spawnPosition, item));
        }
        public ItemEntity? TryPickUp(Vector2Int position)
        {
            if (GetAllItemPositions().Contains(position))
            {
                ItemEntity item = Items.First(item => item.CurrentPosition == position);
                _items.Remove(item);
                return item;
            }
            return null;
        }
        public HashSet<Vector2Int> GetAllItemPositions() => _allItemPositions;
        private HashSet<Vector2Int> _allItemPositions = new();
        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Select(item => item.CurrentPosition).ToHashSet();
        }
    }
}
