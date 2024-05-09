using R3;
using Scripts.Utilities;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    [RequireComponent(typeof(SpriteView), typeof(Animator))]
    public class CharacterView : MonoBehaviour, IDirectional
    {
        public int MoveMilliseconds = 1000;
        public int DashMilliseconds = 1000;
        private const int frame = 16;
        private Func<bool> _isDash;
        private SpriteView _view;
        public Observable<Unit> OnMoveFinished => _onMoveFinished;
        private Subject<Unit> _onMoveFinished = new();
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        private ReactiveProperty<Direction8> _direction = new();
        public Direction8 GetDirection() => Direction.CurrentValue;
        private bool _isVisible => _view.GetVisibility();
        public void Construct(InputReceiver receiver, string characterTypeName)
        {
            _isDash = () => receiver.IsDash;
            _view = GetComponent<SpriteView>();
            RuntimeAnimatorController animation = Addressables
                .LoadAssetAsync<RuntimeAnimatorController>($"Assets/Animations/{characterTypeName}.controller")
                .WaitForCompletion();
            GetComponent<Animator>().runtimeAnimatorController = Instantiate(animation);
        }
        public void Turn(Direction8 direction)
        {
            _direction.OnNext(direction);
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