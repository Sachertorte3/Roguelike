#nullable enable
using R3;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Items
{
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
    public record OnMoveMessage(ItemEntity Entity, Direction8 Direction, Vector2Int Destination);
    public record OnPositionChangedMessage(ItemEntity Character, Vector2Int Direction);
}
