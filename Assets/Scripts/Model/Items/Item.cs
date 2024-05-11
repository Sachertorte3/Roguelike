#nullable enable
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Entities;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;

namespace Scripts.Model.Items
{
    public record Item(Skill Skill)
    {
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await Skill.Use(actor, direction);
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            return Skill.Evaluate(actor, direction);
        }
    }
    public class ItemEntity
    {
        public readonly Item Item;
        private readonly Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }
    }
    internal sealed class ItemFactory
    {
        public ItemEntity CreateItem(Vector2Int spawnPosition, Item item)
        {
            return new ItemEntity(spawnPosition, item);
        }
    }
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
            EntityEvents.OnPositionChanged.Subscribe(_ => SetAllItemPosition());
        }
        public void AddItem(ItemEntity item)
        {
            _items.Add(item);
            EntityEvents.Add(item);
        }
        public void SpawnItem(Vector2Int spawnPosition)
        {
            AddItem(_factory.CreateItem(spawnPosition, new Item(new Skill(10, new LineArea(2)))));
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
        private HashSet<Vector2Int> _allItemPositions = new HashSet<Vector2Int>();
        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Select(item => item.CurrentPosition).ToHashSet();
        }
    }
    public class ItemEntityEvents
    {
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        public Observable<OnMoveMessage> OnMove => _onMove;
        private readonly Subject<OnMoveMessage> _onMove = new();
        public void Add(ItemEntity item)
        {
            item.Position.Subscribe(positionChanged => _onPositionChanged.OnNext(new OnPositionChangedMessage(item, positionChanged)));
            item.OnMove.Subscribe(move => _onMove.OnNext(new OnMoveMessage(item, move.direction, move.destination)));
        }
    }
    public record OnPositionChangedMessage(ItemEntity Character, Vector2Int Direction);
    public record OnMoveMessage(ItemEntity Entity, Direction8 Direction, Vector2Int Destination);
}
