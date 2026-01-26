using System;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Utilities;

namespace View
{
    public class InputReceiver : IDisposable
    {
        private readonly MyInputAction _actions = new();
        private readonly CompositeDisposable _disposables = new();
        private InputActionReference _fieldNavigateRef;
        private InputActionReference _uiNavigateRef;

        public Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();
        public ReadOnlyReactiveProperty<bool> IsDash => _actions.Field.Dash.AsPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsPressedReactiveProperty();

        public Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnSwapItemPerformed => _actions.Field.SwapItem.AsObservable().Select(context => Unit.Default);

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

        public void Enable()
        {
            _actions.Enable();
        }

        public void Disable()
        {
            _actions.Disable();
        }

        public void SwitchMenu()
        {
            Log.Info("[Input] Switch input to Menu");
            _actions.Menu.Enable();
            _actions.Field.Disable();
            SetMove(_actions.UI.Navigate, ref _uiNavigateRef);
        }

        public void SwitchField()
        {
            Log.Info("[Input] Switch input to Field");
            _actions.Field.Enable();
            _actions.Menu.Disable();
            SetMove(_actions.Field.Navigate, ref _fieldNavigateRef);
        }

        private void SetMove(InputAction moveAction, ref InputActionReference cache)
        {
            if (cache == null)
                cache = InputActionReference.Create(moveAction);

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            var uiModule = eventSystem.currentInputModule as InputSystemUIInputModule
                           ?? eventSystem.GetComponent<InputSystemUIInputModule>();
            if (uiModule == null)
                return;

            uiModule.move = cache;
        }
    }
}