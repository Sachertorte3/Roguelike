using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Bonfire : ISerializable<EntityMemento>, IEventEntity
    {
        private Entity _entity;
        public Bonfire(EntityMemento memento)
        {
            _entity = new Entity(memento);
            _events = new();
        }
        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public string ChoiceMessage => "";
        public bool CanBeCanceled => true;
        private readonly List<EntityEvent> _events;
        public IReadOnlyList<EntityEvent> Events => _events;

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }
        public void Destroy()
        {
            _entity.Destroy();
        }
        public void Dispose()
        {
            _entity.Dispose();
        }
        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }
        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }
        public EntityMemento Serialize()
        {
            return _entity.Serialize();
        }
        public static EntityMemento Build(Vector2Int position)
        {
            return Entity.Build(position, EntityLayer.Middle);
        }
    }
}