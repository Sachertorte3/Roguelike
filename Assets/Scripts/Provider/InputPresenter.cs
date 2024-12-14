#nullable enable
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
using IngameDebugConsole;
using R3;
using Utilities;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class InputPresenter
    {
        private readonly ReactiveProperty<bool> _enable = new(false);
        [Inject]
        public InputPresenter(InputReceiver receiver, GameInput input, CharacterControlInputReceiver actionReceiver,
            ChoiceReceiver choiceReceiver, TextInputReceiver textInputReceiver, GameManager gameManager, World world,
            MenuController menuController,
            InventoryView inventoryView)
        {
            var logWindowVisible = Observable.EveryValueChanged(DebugLogManager.Instance, x => x.IsLogWindowVisible).ToReadOnlyReactiveProperty();
            var textInputShown = new ReactiveProperty<bool>(false);
            Observable.CombineLatest(logWindowVisible, actionReceiver.IsEnabled, textInputShown)
                .Subscribe(x =>
                {
                    if (x[0] || !x[1] || x[2])
                        receiver.Disable();
                    else
                        receiver.Enable();
                });

            receiver.OnMovePerformed
                .Select(vector => DirectionMethods.FromVector(vector))
                .Where(direction => direction != null)
                .Subscribe(direction => actionReceiver.SetMoveInput(direction!.Value, true));
            actionReceiver.OnActionRead
                .Select(_ => receiver.MoveVector)
                .Select(vector => DirectionMethods.FromVector(vector))
                .Where(direction => direction != null)
                .Subscribe(direction => actionReceiver.SetMoveInput(direction!.Value, false));
            receiver.OnAttackPerformed.Subscribe(_ => actionReceiver.SetAttackInput());
            receiver.OnThrowPerformed.Subscribe(_ => actionReceiver.SetThrowInput());
            receiver.OnDropPerformed.Subscribe(_ => actionReceiver.SetDropInput());
            receiver.OnDoNothingPerformed.Subscribe(_ => actionReceiver.SetDoNothingInput());
            actionReceiver.OnActionRead
                .Where(_ => receiver.IsDoNothingPerformed)
                .Subscribe(_ => actionReceiver.SetDoNothingInput());
            receiver.OnRenamePerformed.Subscribe(_ => actionReceiver.SetRenameInput());

            receiver.IsDash.Subscribe(isDash => input.SetDash(isDash));
            receiver.IsNoMove.Subscribe(isNoMove => input.SetNoMove(isNoMove));

            var disposable = new SerialDisposable();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map => disposable.Disposable = receiver.IsNoMove.Subscribe(isNoMove =>
                {
                    if (isNoMove)
                    {
                        map.Player.Character.FaceNearestCharacter(map);
                    }
                })
            );

            inventoryView.Focus.Subscribe(focus =>
                actionReceiver.SetItemFocus(focus.ToItemFocus()));

            choiceReceiver.OnShownChoice.Subscribe(async message =>
            {
                var index = await menuController.GetChoice(message.text, message.choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            textInputReceiver.OnShownTextInput.Subscribe(async _ =>
            {
                textInputShown.Value = true;
                var text = await menuController.GetTextInput();
                textInputReceiver.SetTextInput(text);
                textInputShown.Value = false;
            });
        }
    }
}