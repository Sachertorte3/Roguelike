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
    public class Fire : ISerializable<EntityMemento>, IEntity
    {
        private readonly Entity _entity;

        public Fire(EntityMemento memento)
        {
            _entity = new Entity(memento);
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map) => UniTask.CompletedTask;
        public void Destroy() => _entity.Destroy();
        public void Dispose() => _entity.Dispose();
        public void SetVisibility(bool visibility) => _entity.SetVisibility(visibility);
        public void Teleport(Vector2Int position) => _entity.Teleport(position);
        public EntityMemento Serialize() => _entity.Serialize();
        public static EntityMemento Build(Vector2Int position)
        {
            return Entity.Build(position, EntityLayer.Top);
        }
    }
}