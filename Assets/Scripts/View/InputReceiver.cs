using Scripts.Utilities;
using UniRx;
using UnityEngine;

namespace Scripts.View
{
    public class InputReceiver : MonoBehaviour
    {
        private MyInputAction _actions;
        public Vector2 MoveDirection => _actions.Field.Move.ReadValue<Vector2>();
        //public ReadOnlyReactiveProperty<Vector2> MoveDirection => _actions.Field.Move.AsReactiveProperty<Vector2>();
        private void Start()
        {
            _actions = new MyInputAction();
            _actions.Enable();
            //MoveDirection.AddTo(this);
        }
    }
}