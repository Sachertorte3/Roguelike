using System;
using R3;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        private int MoveMilliseconds = 1000;
        private int DashMilliseconds = 1000;
        private Func<bool> _isDash;
        private SpriteView _view;
        private bool _isVisible => _view.GetVisibility();
        private SerialDisposable _disposable = new();

        public void Construct(InputReceiver receiver)
        {
            _isDash = () => receiver.IsDash.CurrentValue;
            _view = GetComponent<SpriteView>();
        }

        public void SetMoveMilliseconds(int moveMilliseconds)
        {
            MoveMilliseconds = moveMilliseconds;
        }

        public void SetDashMilliseconds(int dashMilliseconds)
        {
            DashMilliseconds = dashMilliseconds;
        }

        public void Teleport(Vector2Int position)
        {
            _disposable.Disposable = null;
            transform.position = (Vector3Int)position;
        }

        public void Move(Vector2Int destination, Direction8 direction)
        {
            _disposable.Disposable = null;
            if (_isVisible)
            {
                var position = (Vector3Int)destination - (Vector3Int)direction.Vector();
                var elapsedTime = 0f;
                var totalDuration = (_isDash() ? DashMilliseconds : MoveMilliseconds) / 1000f;

                _disposable.Disposable = Observable.EveryUpdate()
                    .TakeWhile(_ => elapsedTime < totalDuration)
                    .Subscribe(_ =>
                    {
                        elapsedTime += Time.deltaTime;
                        var t = Mathf.Clamp01(elapsedTime / totalDuration);
                        transform.position = Vector3.Lerp(position, (Vector3Int)destination, t);
                    }).AddTo(this);
            }
            else
            {
                transform.position = (Vector3Int)destination;
            }
        }
    }
}