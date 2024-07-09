#nullable enable
using Domain.Model;
using R3;

namespace Domain.Service.Entities
{
    public interface IEntityGroupEvents
    {
        public Observable<(IEntity Entity, OnPositionChangedMessage Message)> OnPositionChanged { get; }
        public Observable<(IEntity Entity, OnMoveMessage Message)> OnMove { get; }
        public Observable<(IEntity Entity, OnTeleportMessage Message)> OnTeleport { get; }
        public Observable<(IEntity Entity, OnDestroyedMessage Message)> OnDestroyed { get; }
    }
}