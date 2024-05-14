using R3;
using UnityEngine;
using Utilities;

namespace View
{
    public class InputReceiver
    {
        private readonly MyInputAction _actions = new();

        public InputReceiver()
        {
            _actions.Field.Enable();
            OnMenuOpening.Subscribe(_ =>
            {
                _actions.Field.Disable();
                _actions.Menu.Enable();
            });
            OnMenuClosing.Subscribe(_ =>
            {
                _actions.Field.Enable();
                _actions.Menu.Disable();
            });
        }

        public Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public bool IsDash => _actions.Field.Dash.IsPressed();
        public bool IsNoMove => _actions.Field.TurnOnly.IsPressed();

        public Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnDropPerformed => _actions.Field.Drop.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuOpening => _actions.Field.OpenMenu.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);
    }
}