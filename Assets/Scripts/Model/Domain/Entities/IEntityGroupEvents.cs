#nullable enable
using R3;
using UnityEngine;
using Utilities;

namespace Model.Entities
{
    public interface IEntityGroupEvents
    {
        public Observable<(Entity Entity, OnPositionChangedMessage Message)> OnPositionChanged { get; }
        public Observable<(Entity Entity, OnMoveMessage Message)> OnMove { get; }
        public Observable<(Entity Entity, OnTeleportMessage Message)> OnTeleport { get; }
    }
    public record OnPositionChangedMessage(Vector2Int Position);
    public record OnMoveMessage(Direction8 Direction, Vector2Int Destination);
    public record OnTeleportMessage(Vector2Int Position);
}