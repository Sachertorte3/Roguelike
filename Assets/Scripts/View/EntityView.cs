using System;
using R3;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        private int ThrowMilliseconds = 1000;
        private int MoveMilliseconds = 1000;
        private int DashMilliseconds = 1000;
        private Func<bool> _isDash;
        private SpriteView _view;
        private bool _isVisible => _view.GetVisibility();
        private SerialDisposable _disposable = new();
        public bool IsMoving { get; private set; }

        public void Construct(InputReceiver receiver)
        {
            _isDash = () => receiver.IsDash.CurrentValue;
            _view = GetComponent<SpriteView>();
        }

        public void SetThrowMilliseconds(int throwMilliseconds)
        {
            ThrowMilliseconds = throwMilliseconds;
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
            IsMoving = false;
        }

        public void Move(Vector2Int destination, Direction8 direction, bool isThrown)
        {
            _disposable.Disposable = null;
            if (_isVisible)
            {
                var position = (Vector3Int)destination - (Vector3Int)direction.Vector();
                var elapsedTime = 0f;

                var totalDuration = 1f;
                if (isThrown)
                    totalDuration = ThrowMilliseconds / 1000f;
                else if (_isDash())
                    totalDuration = DashMilliseconds / 1000f;
                else
                    totalDuration = MoveMilliseconds / 1000f;

                IsMoving = true;

                _disposable.Disposable = Observable.EveryUpdate()
                    .TakeWhile(_ => elapsedTime < totalDuration)
                    .Subscribe(_ =>
                    {
                        if (IsMoving)
                        {
                            elapsedTime += Time.deltaTime;
                            var t = Mathf.Clamp01(elapsedTime / totalDuration);
                            transform.position = Vector3.Lerp(position, (Vector3Int)destination, t);
                        }
                    },
                    _ => IsMoving = false).AddTo(this);
            }
            else
            {
                transform.position = (Vector3Int)destination;
            }
        }
    }
}