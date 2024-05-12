#nullable enable
using R3;
using Scripts.Model;
using Scripts.Model.Characters.Behavior;
using Scripts.Utilities;
using Scripts.View;
using Scripts.View.UI;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Scripts.Provider
{
    public class InputPresenter
    {
        [Inject]
        public InputPresenter(InputReceiver receiver, CharacterControllInputReceiver actionReceiver, InventoryView inventoryView)
        {
            receiver.OnMovePerformed
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, true);
                });
            actionReceiver.OnActionRead.Select(_ => receiver.MoveVector)
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, false);
                });
            receiver.OnAttackPerformed.Subscribe(_ =>
            {
                actionReceiver.SetAttackInput();
            });
            inventoryView.OnFocusChanged.Subscribe(index =>
            {
                actionReceiver.SetInventoryIndex(index);
            });

            Globals.IsDash = () => receiver.IsDash;
            Globals.IsNoMove = () => receiver.IsNoMove;
        }
    }
}