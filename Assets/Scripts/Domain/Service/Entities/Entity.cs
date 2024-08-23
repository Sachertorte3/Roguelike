using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using R3;
using UnityEngine;
using Utilities;
using Domain.Model;

namespace Domain.Service.Entities
{
    internal class Entity : IDisposable, ISerializable<EntityMemento>
    {
        public readonly Id<IEntity> Id;
        private readonly EntityLayer _layer;
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        private readonly Subject<Vector2Int> _onTeleport = new();
        private readonly ReactiveProperty<Vector2Int> _position;
        private readonly ReactiveProperty<bool> _visibleByPlayer = new(false);
        private readonly Subject<Unit> _onDestroyed = new();

        public Entity(EntityMemento data)
        {
            Id = new Id<IEntity>(data.Id);
            _position = new ReactiveProperty<Vector2Int>(data.Position);
            _layer = data.Layer;
        }

        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        public Observable<Vector2Int> OnTeleport => _onTeleport;
        public ReadOnlyReactiveProperty<bool> VisibleByPlayer => _visibleByPlayer;
        public EntityLayer Layer => _layer;
        public Observable<Unit> OnDestroyed => _onDestroyed;

        public void Dispose()
        {
            _position.Dispose();
            _onMove.Dispose();
        }

        public EntityMemento Serialize()
        {
            return new EntityMemento
            {
                Id = Id.Value,
                Position = _position.CurrentValue,
                Layer = _layer
            };
        }

        public static EntityMemento Build(Vector2Int position, EntityLayer layer)
        {
            return new EntityMemento
            {
                Id = UniqueIdGenerator.Generate<IEntity>().Value,
                Position = position,
                Layer = layer
            };
        }

        public void SetVisibility(bool visible)
        {
            _visibleByPlayer.Value = visible;
        }

        public void Teleport(Vector2Int position)
        {
            _position.Value = position;
            _onTeleport.OnNext(position);
        }

        public async UniTask Move(Direction8 direction, int moveMilliseconds)
        {
            _position.Value += direction.Vector();
            _onMove.OnNext((direction, CurrentPosition));
            if (VisibleByPlayer.CurrentValue) await UniTask.Delay(moveMilliseconds);
        }

        public void Destroy()
        {
            _onDestroyed.OnNext(Unit.Default);
        }
    }
}