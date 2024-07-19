#nullable enable
using Domain.Service.Characters.Behavior;
using Model.Game;
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
            World world, InventoryView inventoryView)
        {
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
            receiver.OnDropPerformed.Subscribe(_ => world.HandleItemDrop(inventoryView.CurrentFocus));

            receiver.IsDash.Subscribe(isDash => input.SetDash(isDash));
            receiver.IsNoMove.Subscribe(isNoMove => input.SetNoMove(isNoMove));

            inventoryView.OnFocusChanged.Subscribe(index => actionReceiver.SetInventoryIndex(index));
        }
    }
}