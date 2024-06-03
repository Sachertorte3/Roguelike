using System.Linq;
using Cysharp.Threading.Tasks;
using Data.Setting;
using Model.Domain.Action;
using Unity.Logging;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character, IMap world)
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

        public Move Filter(Move move, IHasBehavior character, bool started, IMap world, IInput input)
        {
            move = MoveFilter(move, character, world);
            move = DashFilter(move, character, started, world, input);
            return move;
        }

        private Move MoveFilter(Move move, IHasBehavior character, IMap world)
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

        private Move DashFilter(Move move, IHasBehavior character, bool started, IMap world, IInput input)
        {
            if (!IsDashingStraight(started, input)) return move;
            var canMoveDirections = DirectionMethods.AllDirections
                .Where(direction => character.CanMove(direction, world))
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

        public bool IsDashingStraight(bool started, IInput input)
        {
            return input.IsDash() && !started;
        }
    }
}