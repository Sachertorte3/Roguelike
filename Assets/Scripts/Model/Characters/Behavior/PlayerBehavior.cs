#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Model.Action;
using Model.Effect;
using Model.Setting;
using Unity.Logging;
using Utilities;

namespace Model.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControllInputReceiver _receiver;

        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            if (Globals.IsDash()) await _intelligentDashController.Wait(character);

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
                        (var move, var started) = firstCompletedTask.result1;
                        if (Globals.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value)
                                move = _intelligentDashController.Filter(move, character, started);

                            if (move.Doable(character))
                                return move;
                            character.Turn(move.Direction);
                        }

                        break;
                    case 1:
                        var itemIndex = firstCompletedTask.result2;
                        var item = character.Inventory.GetItem(itemIndex);
                        IAction action;
                        if (item == null)
                            action = new UseSkill(new Skill(new SkillData(new LineArea(1, false), new AttackEffect(1))),
                                character.CurrentDirection);
                        else
                            action = new UseItem(itemIndex, character.CurrentDirection);

                        if (action.Doable(character)) return action;
                        break;
                    case 2:
                        itemIndex = firstCompletedTask.result3;
                        if (character.Inventory.GetItem(itemIndex) != null)
                        {
                            action = new ThrowItem(itemIndex, character.CurrentDirection);
                            if (action.Doable(character)) return action;
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

    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character)
        {
            if (character.CanMove(character.CurrentDirection) &&
                character.CanMove(character.CurrentDirection.Reverse()) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise()) &&
                     !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise()))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise()) &&
                        !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise()))
                )
               )
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
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
                var directionRotateClockwise = move.Direction.Rotate45Clockwise();
                var canMoveDirectionRotateClockwise = character.CanMove(directionRotateClockwise);
                var directionRotateAntiClockwise = move.Direction.Rotate45AntiClockwise();
                var canMoveDirectionRotateAntiClockwise = character.CanMove(directionRotateAntiClockwise);
                if (canMoveDirectionRotateClockwise && !canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateClockwise);
                if (!canMoveDirectionRotateClockwise && canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateAntiClockwise);
            }

            return move;
        }

        private Move DashFilter(Move move, IHasBehavior character, bool started)
        {
            if (!IsDashingStraight(started)) return move;
            var canMoveDirections = DirectionMethods.AllDirections.Where(direction => character.CanMove(direction))
                .ToHashSet();
            var inStraightway = canMoveDirections.Count() == 2;
            if (inStraightway)
            {
                var lastMoveDirection = character.CurrentDirection;
                if (!canMoveDirections.Remove(lastMoveDirection.Reverse()))
                {
                    Log.Info(
                        $"The possible position of the previous turn based on the character's direction({character.CurrentDirection}) is not a passage.");
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