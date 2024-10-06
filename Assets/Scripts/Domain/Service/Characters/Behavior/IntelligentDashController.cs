using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Setting;
using Domain.Service.Action;
using Unity.Logging;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character, IMap map)
        {
            if (character.CanMove(character.CurrentDirection, map) &&
                character.CanMove(character.CurrentDirection.Reverse(), map) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise(), map) &&
                     !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise(), map))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise(), map) &&
                        !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise(), map))
                )
               )
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
        }

        public Move Filter(Move move, IHasBehavior character, bool started, IMap map, IInput input)
        {
            move = MoveFilter(move, character, map);
            move = DashFilter(move, character, started, map, input);
            return move;
        }

        private Move MoveFilter(Move move, IHasBehavior character, IMap map)
        {
            if (!character.CanMove(move.Direction, map) && move.Direction.IsDiagonal())
            {
                var directionRotateClockwise = move.Direction.Rotate45Clockwise();
                var canMoveDirectionRotateClockwise = character.CanMove(directionRotateClockwise, map);
                var directionRotateAntiClockwise = move.Direction.Rotate45AntiClockwise();
                var canMoveDirectionRotateAntiClockwise = character.CanMove(directionRotateAntiClockwise, map);
                if (canMoveDirectionRotateClockwise && !canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateClockwise);
                if (!canMoveDirectionRotateClockwise && canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateAntiClockwise);
            }

            return move;
        }

        private Move DashFilter(Move move, IHasBehavior character, bool started, IMap map, IInput input)
        {
            if (!IsDashingStraight(started, input)) return move;
            var canMoveDirections = DirectionMethods.AllDirections
                .Where(direction => character.CanMove(direction, map))
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