#nullable enable
using R3;

namespace Model.Domain.Entities
{
    public interface IEntityGroupEvents
    {
        public Observable<(Entity Entity, OnPositionChangedMessage Message)> OnPositionChanged { get; }
        public Observable<(Entity Entity, OnMoveMessage Message)> OnMove { get; }
        public Observable<(Entity Entity, OnTeleportMessage Message)> OnTeleport { get; }
    }
}