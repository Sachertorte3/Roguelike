using System;
using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Setting;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Entities
{
    public class Entity: IDisposable
    {
        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        private readonly ReactiveProperty<Vector2Int> _position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        internal bool VisibleByPlayer = false;
        public Entity(Vector2Int position)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
        }
        public void Dispose()
        {
            _position.Dispose();
            _onMove.Dispose();
        }
        public async UniTask Move(Direction8 direction)
        {
            _position.Value += direction.Vector();
            _onMove.OnNext((direction, CurrentPosition));
            if (VisibleByPlayer)
            {
                await UniTask.Delay(Globals.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
            }
        }
    }
}
