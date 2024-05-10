using R3;
using Scripts.Utilities;
using System;
using UnityEngine;

namespace Scripts.View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        public int MoveMilliseconds = 1000;
        public int DashMilliseconds = 1000;
        private SpriteView _view;
        private const int frame = 16;
        private Func<bool> _isDash;
        public Observable<Unit> OnMoveFinished => _onMoveFinished;
        private Subject<Unit> _onMoveFinished = new();
        private bool _isVisible => _view.GetVisibility();
        public void Construct(InputReceiver receiver)
        {
            _isDash = () => receiver.IsDash;
            _view = GetComponent<SpriteView>();
        }
        public void Move(Vector2Int destination, Direction8 direction)
        {
            if (_isVisible)
            {
                Vector3Int position = (Vector3Int)destination - (Vector3Int)direction.Vector();
                Observable.Interval(TimeSpan.FromSeconds((_isDash() ? DashMilliseconds : MoveMilliseconds) / 1000f * 0.75f / frame))
                .Take(frame)
                .Index()
                .Subscribe(l =>
                {
                    transform.position = Vector3.Lerp(position, (Vector3Int)destination, (l + 1) / (float)frame);
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