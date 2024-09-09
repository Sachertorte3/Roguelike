#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Model.Message;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    internal class ItemEntity : IItemEntity
    {
        private readonly Entity _entity;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();

        public ItemEntity(ItemEntityMemento item)
        {
            Item = new Item(item.Item);
            _entity = new Entity(item.Entity);
        }

        public IItem Item { get; init; }

        public Sprite Icon => Item.Icon;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _entity.Dispose();
            _onEffectSpawned.Dispose();
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public ItemEntityMemento Serialize()
        {
            return new ItemEntityMemento
            {
                Item = Item.Serialize(),
                Entity = _entity.Serialize()
            };
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, IMap map)
        {
            var result = position;

            while (map.IsPassable(result + direction.Vector()))
            {
                result += direction.Vector();
            }

            if (map.IsMapPassable(position + direction.Vector()))
            {
                result += direction.Vector();
            }

            return result;
        }

        public static float EvaluateThrow(IItem item, Vector2Int position, IActor actor, Direction8 direction, IMap map)
        {
            if (item.CanActivateWhenThrown)
                return 0;

            var destination = GetThrowDestination(position, direction, map);

            return item.EvaluateWhenThrown(actor, destination, direction, map);
        }

        ~ItemEntity()
        {
            Dispose();
        }
    }
}