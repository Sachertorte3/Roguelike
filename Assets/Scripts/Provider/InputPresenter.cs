#nullable enable
using Model;
using Model.Characters.Behavior;
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
        public InputPresenter(InputReceiver receiver, CharacterControllInputReceiver actionReceiver, World world, InventoryView inventoryView)
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
                var item = world.Player.Inventory.GetItem(inventoryView.CurrentFocus);
                if (item != null)
                {
                    var itemEntity = world.ActiveMap.CurrentValue.ItemManager.TryPickUp(world.Player.CurrentPosition);
                    world.Player.ReplaceInventory(itemEntity?.Item, inventoryView.CurrentFocus);
                    world.ActiveMap.CurrentValue.ItemManager.SpawnItem(item, world.Player.CurrentPosition);
                }
            });
            inventoryView.OnFocusChanged.Subscribe(index => { actionReceiver.SetInventoryIndex(index); });

            Globals.IsDash = () => receiver.IsDash;
            Globals.IsNoMove = () => receiver.IsNoMove;
        }
    }
}