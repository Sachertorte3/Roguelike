#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Model.Action;
using Model.Domain;
using Model.Effect;
using Model.Setting;
using System;
using System.Linq;
using Unity.Logging;
using Utilities;

namespace Model.Characters.Behavior
{
    public sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControllInputReceiver _receiver;

        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IWorld world)
        {
            if (world.IsDash()) await _intelligentDashController.Wait(character, world);

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
                        if (world.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value)
                                move = _intelligentDashController.Filter(move, character, started, world);

                            if (move.Doable(character, world))
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

    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character, IWorld world)
        {
            if (character.CanMove(character.CurrentDirection, world) &&
                character.CanMove(character.CurrentDirection.Reverse(), world) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise(), world) &&
                     !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise(), world))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise(), world) &&
                        !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise(), world))
                )
               )
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
        }

        public Move Filter(Move move, IHasBehavior character, bool started, IWorld world)
        {
            move = MoveFilter(move, character, world);
            move = DashFilter(move, character, started, world);
            return move;
        }

        private Move MoveFilter(Move move, IHasBehavior character, IWorld world)
        {
            if (!character.CanMove(move.Direction, world))
            {
                var directionRotateClockwise = move.Direction.Rotate45Clockwise();
                var canMoveDirectionRotateClockwise = character.CanMove(directionRotateClockwise, world);
                var directionRotateAntiClockwise = move.Direction.Rotate45AntiClockwise();
                var canMoveDirectionRotateAntiClockwise = character.CanMove(directionRotateAntiClockwise, world);
                if (canMoveDirectionRotateClockwise && !canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateClockwise);
                if (!canMoveDirectionRotateClockwise && canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateAntiClockwise);
            }

            return move;
        }

        private Move DashFilter(Move move, IHasBehavior character, bool started, IWorld world)
        {
            if (!IsDashingStraight(started, world)) return move;
            var canMoveDirections = DirectionMethods.AllDirections.Where(direction => character.CanMove(direction, world))
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

        public bool IsDashingStraight(bool started, IWorld world)
        {
            return world.IsDash() && !started;
        }
    }
}