#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Scripts.Model.Characters;
using UnityEngine;
using VContainer;

namespace Scripts.Model.Items
{
    public sealed class ItemManager
    {
        private ObservableList<ItemEntity> _items = new();
        public ReadOnlyCollection<ItemEntity> Items => new(_items);
        public Observable<ItemEntity> OnItemAdded => _items.ObserveAdd().Select(item => item.Value);
        public Observable<ItemEntity> OnItemRemoved => _items.ObserveRemove().Select(item => item.Value);
        private ItemFactory _factory = new();
        public ItemEntityEvents ItemEntityEvents = new();
        [Inject]
        public ItemManager(CharacterManager characterManager)
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            ItemEntityEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllItemPosition();
                positionChanged.Item.SetVisiblity(characterManager.Player.Area.Get().Contains(positionChanged.Position));
            });
            ItemEntityEvents.OnDisabled.Subscribe(dead => _items.Remove(dead.Item));

            characterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var item in _items)
                {
                    if (areaChanged.AreaExited.Contains(item.CurrentPosition))
                    {
                        item.SetVisiblity(false);
                    }
                    else if (areaChanged.AreaEntered.Contains(item.CurrentPosition))
                    {
                        item.SetVisiblity(true);
                    }
                }
            });
        }
        public void AddItem(ItemEntity item)
        {
            _items.Add(item);
            ItemEntityEvents.Add(item);
        }
        public ItemEntity SpawnItem(Item item, Vector2Int spawnPosition)
        {
            ItemEntity itemEntity = _factory.CreateItem(spawnPosition, item);
            AddItem(itemEntity);
            return itemEntity;
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
