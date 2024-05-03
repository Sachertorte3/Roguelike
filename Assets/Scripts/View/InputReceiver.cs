using R3;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.View
{
    public class InputReceiver
    {
        private MyInputAction _actions = new MyInputAction();
        public Observable<Vector2> OnMovePerformed => _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());
        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public Observable<Unit> OnMenuOpening => _actions.Field.OpenMenu.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);
        public InputReceiver()
        {
            _actions.Field.Enable();
            OnMenuOpening.Subscribe(_ => { _actions.Field.Disable(); _actions.Menu.Enable(); });
            OnMenuClosing.Subscribe(_ => { _actions.Field.Enable(); _actions.Menu.Disable(); });
        }
    }
}