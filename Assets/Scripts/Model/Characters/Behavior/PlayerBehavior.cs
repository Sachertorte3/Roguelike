#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Items;
using Scripts.Model.Setting;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private CharacterControllInputReceiver _receiver;
        private IntelligentDashController _intelligentDashController = new();
        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }
        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            if (Globals.IsDash())
            {
                await _intelligentDashController.Wait(character);
            }

            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            UniTask<int> itemTask = _receiver.OnItemActionReceived.WaitAsync();

            _receiver.ReadInput();

            var firstCompletedTask = await UniTask.WhenAny(moveTask, itemTask);
            while (true)
            {
                switch (firstCompletedTask.winArgumentIndex)
                {
                    case 0:
                        (Move move, bool started) = firstCompletedTask.result1;
                        if (Globals.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value)
                            {
                                move = _intelligentDashController.Filter(move, character, started);
                            }

                            if (move.Doable(character))
                            {
                                return move;
                            }
                            else
                            {
                                character.Turn(move.Direction);
                            }
                        }
                        break;
                    case 1:
                        Item? item = character.Inventory.Items[firstCompletedTask.result2];
                        IAction action;
                        if (item == null)
                        {
                            action = new UseSkill(new Skill(10, new LineArea(1)), character.CurrentDirection);
                        }
                        else
                        {
                            action = new UseItem(item, character.CurrentDirection);
                        }

                        if (action.Doable(character))
                        {
                            return action;
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                moveTask = _receiver.OnMoveInputReceived.WaitAsync();
                itemTask = _receiver.OnItemActionReceived.WaitAsync();
                firstCompletedTask = await UniTask.WhenAny(moveTask, itemTask);
            }
        }
    }
    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character)
        {
            if (character.CanMove(character.CurrentDirection) && character.CanMove(character.CurrentDirection.Reverse()) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise()))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise()))
                )
            )
            {
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
            }
        }
        public Move Filter(Move move, IHasBehavior character, bool started)
        {
            move = MoveFilter(move, character);
            move = DashFilter(move, character, started);
            return move;
        }
        private Move MoveFilter(Move move, IHasBehavior character)
        {
            if (!character.CanMove(move.Direction))
            {
                Direction8 directionRotateClockwise = move.Direction.Rotate45Clockwise();
                bool canMoveDirectionRotateClockwise = character.CanMove(directionRotateClockwise);
                Direction8 directionRotateAntiClockwise = move.Direction.Rotate45AntiClockwise();
                bool canMoveDirectionRotateAntiClockwise = character.CanMove(directionRotateAntiClockwise);
                if (canMoveDirectionRotateClockwise && !canMoveDirectionRotateAntiClockwise)
                {
                    return new Move(directionRotateClockwise);
                }
                else if (!canMoveDirectionRotateClockwise && canMoveDirectionRotateAntiClockwise)
                {
                    return new Move(directionRotateAntiClockwise);
                }
            }
            return move;
        }
        private Move DashFilter(Move move, IHasBehavior character, bool started)
        {
            if (!IsDashingStraight(started))
            {
                return move;
            }
            HashSet<Direction8> canMoveDirections = DirectionMethods.AllDirections.Where(direction => character.CanMove(direction)).ToHashSet();
            bool inStraightway = canMoveDirections.Count() == 2;
            if (inStraightway)
            {
                Direction8 lastMoveDirection = character.CurrentDirection;
                if (!canMoveDirections.Remove(lastMoveDirection.Reverse()))
                {
                    Log.Info($"The possible position of the previous turn based on the character's direction({character.CurrentDirection}) is not a passage.");
                    return move;
                }
                return new Move(canMoveDirections.First());
            }
            return move;
        }
        public bool IsDashingStraight(bool started)
        {
            return Globals.IsDash() && !started;
        }
    }
}
