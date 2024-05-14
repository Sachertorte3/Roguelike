#nullable enable
using Model;
using Model.Characters;
using Model.Characters.Behavior;
using Model.Items;
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
        public InputPresenter(InputReceiver receiver, CharacterControllInputReceiver actionReceiver,
            CharacterManager characterManager, ItemManager itemManager, InventoryView inventoryView)
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
            receiver.OnAttackPerformed.Subscribe(_ => { actionReceiver.SetAttackInput(); });
            receiver.OnThrowPerformed.Subscribe(_ => { actionReceiver.SetThrowInput(); });
            receiver.OnDropPerformed.Subscribe(_ =>
            {
                var item = characterManager.Player.Inventory.GetItem(inventoryView.CurrentFocus);
                if (item != null)
                {
                    var itemEntity = itemManager.TryPickUp(characterManager.Player.CurrentPosition);
                    characterManager.Player.ReplaceInventory(itemEntity?.Item, inventoryView.CurrentFocus);
                    itemManager.SpawnItem(item, characterManager.Player.CurrentPosition);
                }
            });
            inventoryView.OnFocusChanged.Subscribe(index => { actionReceiver.SetInventoryIndex(index); });

            Globals.IsDash = () => receiver.IsDash;
            Globals.IsNoMove = () => receiver.IsNoMove;
        }
    }
}