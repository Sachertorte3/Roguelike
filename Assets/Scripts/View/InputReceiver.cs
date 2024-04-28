using UniRx;
using UnityEngine;
using Scripts.Utilities;

namespace Scripts.View
{
    public class InputReceiver : MonoBehaviour
    {
        MyInputAction _actions;
        public ReadOnlyReactiveProperty<Vector2> MoveDirection => _actions.Field.Move.AsReactiveProperty<Vector2>();
        private void Start()
        {
            _actions = new MyInputAction();
            _actions.Enable();
            MoveDirection.AddTo(this);
        }
    }
}