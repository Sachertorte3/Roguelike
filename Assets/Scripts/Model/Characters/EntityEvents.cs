#nullable enable
using R3;
using Scripts.Utilities;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Entities
{
    public class EntityEvents
    {
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        public Observable<OnMoveMessage> OnMove => _onMove;
        private readonly Subject<OnMoveMessage> _onMove = new();
        public void Add(Entity entity)
        {
            _onPositionChanged.Merge(entity.Position.Select(positionChanged => new OnPositionChangedMessage(entity, positionChanged)));
            _onMove.Merge(entity.OnMove.Select(move => new OnMoveMessage(entity, move.direction, move.destination)));
        }
    }
    public record OnPositionChangedMessage(Entity Character, Vector2Int Direction);
    public record OnMoveMessage(Entity Character, Direction8 Direction, Vector2Int Destination);
}
