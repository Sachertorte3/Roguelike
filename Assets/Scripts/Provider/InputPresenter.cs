#nullable enable
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using IngameDebugConsole;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class InputPresenter
    {
        [Inject]
        public InputPresenter(InputReceiver receiver, GameInput input, CharacterControlInputReceiver actionReceiver,
            ChoiceReceiver choiceReceiver, GameManager gameManager, World world, MenuController menuController, InventoryView inventoryView)
        {
            Observable.EveryValueChanged(DebugLogManager.Instance, x => x.IsLogWindowVisible)
                .Subscribe(x =>
                {
                    if (x)
                        receiver.Disable();
                    else
                        receiver.Enable();
                });
            receiver.OnMovePerformed
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    var direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, true);
                });
            actionReceiver.OnActionRead.Select(_ => receiver.MoveVector)
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    var direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, false);
                });
            receiver.OnAttackPerformed.Subscribe(_ => actionReceiver.SetAttackInput());
            receiver.OnThrowPerformed.Subscribe(_ => actionReceiver.SetThrowInput());
            receiver.OnDropPerformed.Subscribe(_ =>
            {
                if (inventoryView.CurrentFocus.HasValue)
                {
                    world.HandleItemDrop(inventoryView.CurrentFocus.Value);
                }
            });

            receiver.IsDash.Subscribe(isDash => input.SetDash(isDash));
            receiver.IsNoMove.Subscribe(isNoMove => input.SetNoMove(isNoMove));

            var disposable = new SerialDisposable();
            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => disposable.Disposable = receiver.IsNoMove.Subscribe(isNoMove =>
                {
                    if (isNoMove)
                    {
                        map.Player.FaceNearestCharacter(map);
                    }
                })
            );

            inventoryView.OnFocusChanged.Subscribe(index => actionReceiver.SetInventoryIndex(index));

            choiceReceiver.OnShownChoice.Subscribe(async message =>
            {
                var index = await menuController.GetChoice(message.text, message.choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            receiver.OnQuickSave.Subscribe(_ => gameManager.Save());
            receiver.OnQuickLoad.Subscribe(_ => gameManager.LoadAndStart());
        }
    }
}