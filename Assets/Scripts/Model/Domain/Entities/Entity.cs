using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Entities
{
    public class Entity : IDisposable
    {
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        private readonly Subject<Vector2Int> _onTeleport = new();
        private readonly ReactiveProperty<Vector2Int> _position;
        private readonly ReactiveProperty<bool> _visibleByPlayer = new(false);

        public Entity(Vector2Int position)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
        }

        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        public Observable<Vector2Int> OnTeleport => _onTeleport;
        public ReadOnlyReactiveProperty<bool> VisibleByPlayer => _visibleByPlayer;

        public void Dispose()
        {
            _position.Dispose();
            _onMove.Dispose();
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
    }
}