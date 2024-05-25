using R3;
using System;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        private const int frame = 16;
        public int MoveMilliseconds = 1000;
        public int DashMilliseconds = 1000;
        private Func<bool> _isDash;
        private readonly Subject<Unit> _onMoveFinished = new();
        private SpriteView _view;
        public Observable<Unit> OnMoveFinished => _onMoveFinished;
        private bool _isVisible => _view.GetVisibility();

        public void Construct(InputReceiver receiver)
        {
            _isDash = () => receiver.IsDash.CurrentValue;
            _view = GetComponent<SpriteView>();
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
                Observable.Interval(TimeSpan.FromSeconds((_isDash() ? DashMilliseconds : MoveMilliseconds) / 1000f *
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
                transform.position = (Vector3Int)destination;
            }
        }
    }
}