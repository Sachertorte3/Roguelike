using Scripts.Utilities;
using System;
using UniRx;
using UnityEngine;

namespace Scripts.View
{
    public class InputReceiver
    {
        private MyInputAction _actions = new MyInputAction();
        public IObservable<Vector2> OnMovePerformed => _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());
        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public InputReceiver()
        {
            _actions.Enable();
        }
    }
}