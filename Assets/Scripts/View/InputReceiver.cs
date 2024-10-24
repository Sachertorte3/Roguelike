using System;
using R3;
using UnityEngine;
using Utilities;

namespace View
{
    public class InputReceiver : IDisposable
    {
        private readonly MyInputAction _actions = new();
        private readonly CompositeDisposable _disposables = new();

        public InputReceiver()
        {
            _actions.Field.Enable();
            _disposables.Add(OnMenuOpening.Subscribe(_ =>
            {
                _actions.Field.Disable();
                _actions.Menu.Enable();
            }));
            _disposables.Add(OnMenuClosing.Subscribe(_ =>
            {
                _actions.Field.Enable();
                _actions.Menu.Disable();
            }));
        }

        public Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public ReadOnlyReactiveProperty<bool> IsDash => _actions.Field.Dash.AsPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsPressedReactiveProperty();

        public Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnDropPerformed => _actions.Field.Drop.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuOpening => _actions.Field.OpenMenu.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnQuickSave => _actions.Field.QuickSave.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnQuickLoad => _actions.Field.QuickLoad.AsObservable().Select(context => Unit.Default);

        public void Dispose()
        {
            _disposables.Dispose();
        }

        ~InputReceiver()
        {
            Dispose();
        }
    }
}