#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Characters;
using Domain.Model.Setting;
using Domain.Service.Action;
using Unity.Logging;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControllInputReceiver _receiver;

        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public bool WanderAround => true;

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            Log.Debug("[PlayerThink] Start waiting input...");
            if (input.IsDash()) await _intelligentDashController.Wait(character, world);

            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();

            _receiver.ReadInput();

            var firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask);
            while (true)
            {
                switch (firstCompletedTask.winArgumentIndex)
                {
                    case 0:
                        var (move, started) = firstCompletedTask.result1;
                        if (input.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value)
                                move = _intelligentDashController.Filter(move, character, started, world, input);

                            var swap = new Swap(move.Direction);
                            character.Turn(move.Direction);
                            if (move.Doable(character, world))
                                return move;
                            else if (world.IsTouchableEventEntityAt(character.CurrentPosition + move.Direction.Vector(), EntityLayer.Middle))
                            {
                                world.Touch(character.CurrentPosition + move.Direction.Vector());
                                return new DoNothing();
                            }
                            else if (swap.Doable(character, world))
                                return swap;
                        }

                        break;
                    case 1:
                        var itemIndex = firstCompletedTask.result2;
                        var item = character.Inventory.GetItem(itemIndex);
                        IAction action;

                        if (item == null)
                            action = new UseSkill(character.Skills[0], character.CurrentDirection);
                        else
                            action = new UseItem(itemIndex, character.CurrentDirection);

                        if (action.Doable(character, world)) return action;
                        break;
                    case 2:
                        itemIndex = firstCompletedTask.result3;
                        if (character.Inventory.GetItem(itemIndex) != null)
                        {
                            action = new ThrowItem(itemIndex, character.CurrentDirection);
                            if (action.Doable(character, world)) return action;
                        }

                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                moveTask = _receiver.OnMoveInputReceived.WaitAsync();
                useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
                throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();
                firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask);
            }
        }
    }
}