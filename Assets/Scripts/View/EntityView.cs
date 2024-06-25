using System;
using R3;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        private const int frame = 16;
        private int MoveMilliseconds = 1000;
        private int DashMilliseconds = 1000;
        private readonly Subject<Unit> _onMoveFinished = new();
        private Func<bool> _isDash;
        private SpriteView _view;
        public Observable<Unit> OnMoveFinished => _onMoveFinished;
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
            transform.position = (Vector3Int)position;
        }

        public void Move(Vector2Int destination, Direction8 direction)
        {
            if (_isVisible)
            {
                var position = (Vector3Int)destination - (Vector3Int)direction.Vector();
                _disposable.Disposable = Observable.Interval(TimeSpan.FromSeconds((_isDash() ? DashMilliseconds : MoveMilliseconds) / 1000f *
                        0.75f / frame))
                    .Take(frame)
                    .Index()
                    .Subscribe(
                        l =>
                        {
                            transform.position =
                                Vector3.Lerp(position, (Vector3Int)destination, (l + 1) / (float)frame);
                        },
                        _ => _onMoveFinished.OnNext(Unit.Default)).AddTo(this);
            }
            else
            {
                _disposable.Dispose();
                transform.position = (Vector3Int)destination;
            }
        }
    }
}