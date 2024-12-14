using System;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace View
{
    public class InputReceiver : IDisposable
    {
        private readonly MyInputAction _actions = new();
        private readonly CompositeDisposable _disposables = new();

        public Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public ReadOnlyReactiveProperty<bool> IsDash => _actions.Field.Dash.AsPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsPressedReactiveProperty();

        public Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnDropPerformed => _actions.Field.Drop.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnDoNothingPerformed =>
            _actions.Field.DoNothing.AsObservable().Select(context => Unit.Default);

        public bool IsDoNothingPerformed => _actions.Field.DoNothing.IsPressed();

        public Observable<Unit> OnRenamePerformed =>
            _actions.Field.Rename.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnMainMenuOpening => _actions.Field.OpenMainMenu.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);

        public void Dispose()
        {
            _disposables.Dispose();
        }

        ~InputReceiver()
        {
            Dispose();
        }

        public void Enable()
        {
            Debug.Log("Enable");
            _actions.Enable();
        }

        public void Disable()
        {
            Debug.Log("Disable");
            _actions.Disable();
        }
    }
}