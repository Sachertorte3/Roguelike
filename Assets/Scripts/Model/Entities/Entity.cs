using R3;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Entities
{
    public class Entity
    {
        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        private readonly ReactiveProperty<Vector2Int> _position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        public Entity(Vector2Int position)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
        }
        public void Move(Direction8 direction)
        {
            _position.Value += direction.Vector();
            _onMove.OnNext((direction, CurrentPosition));
        }
    }
}
