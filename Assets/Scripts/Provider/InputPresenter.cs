#nullable enable
using R3;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Items;
using Scripts.Utilities;
using Scripts.View;
using Scripts.View.UI;
using System;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Scripts.Provider
{
    public class InputPresenter
    {
        [Inject]
        public InputPresenter(InputReceiver receiver, CharacterControllInputReceiver actionReceiver, CharacterManager characterManager, ItemManager itemManager, InventoryView inventoryView)
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
            receiver.OnThrowPerformed.Subscribe(_ =>
            {
                actionReceiver.SetThrowInput();
            });
            receiver.OnDropPerformed.Subscribe(_ =>
            {
                Item? item = characterManager.Player.Inventory.Items[inventoryView.CurrentFocus];
                if (item != null)
                {
                    ItemEntity? itemEntity = itemManager.TryPickUp(characterManager.Player.CurrentPosition);
                    characterManager.Player.ReplaceInventory(itemEntity?.Item, inventoryView.CurrentFocus);
                    itemManager.SpawnItem(item, characterManager.Player.CurrentPosition);
                }
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