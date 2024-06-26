using System.Collections.Generic;
using System.Linq;
using Domain.Service.Action;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class Wander : IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world)
        {
            return GenerateMoveActionsDoable(character, world).Cast<IAction>()
                .Concat(GenerateUseSkillActionsDoable(character, world));
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character, IMap world)
        {
            var directions = new Dictionary<Direction8, float>();
            var facingDirection = character.CurrentDirection;
            if (character.CanMove(facingDirection, world))
            {
                directions.Add(facingDirection, 0.1f);
                if (!world.IsMapPassable(character.CurrentPosition +
                                         facingDirection.RotateClockwise(new Angle(135)).Vector())
                    && character.CanMove(facingDirection.RotateClockwise(new Angle(90)), world))
                {
                    directions.Add(facingDirection.RotateClockwise(new Angle(90)), 0.1f);
                }

                if (!world.IsMapPassable(character.CurrentPosition +
                                         facingDirection.RotateAntiClockwise(new Angle(135)).Vector())
                    && character.CanMove(facingDirection.RotateAntiClockwise(new Angle(90)), world))
                {
                    directions.Add(facingDirection.RotateAntiClockwise(new Angle(90)), 0.1f);
                }
            }
            else
            {
                if (character.CanMove(facingDirection.Rotate90Clockwise(), world))
                {
                    directions.Add(facingDirection.Rotate90Clockwise(), 0.1f);
                }

                if (character.CanMove(facingDirection.Rotate90AntiClockwise(), world))
                {
                    directions.Add(facingDirection.Rotate90AntiClockwise(), 0.1f);
                }

                if (character.CanMove(facingDirection.Rotate45Clockwise(), world))
                {
                    directions.Add(facingDirection.Rotate45Clockwise(), 0.05f);
                }

                if (character.CanMove(facingDirection.Rotate45AntiClockwise(), world))
                {
                    directions.Add(facingDirection.Rotate45AntiClockwise(), 0.05f);
                }

                if (character.CanMove(facingDirection.Reverse().Rotate45Clockwise(), world))
                {
                    directions.Add(facingDirection.Reverse().Rotate45Clockwise(), 0.02f);
                }

                if (character.CanMove(facingDirection.Reverse().Rotate45AntiClockwise(), world))
                {
                    directions.Add(facingDirection.Reverse().Rotate45AntiClockwise(), 0.02f);
                }

                if (character.CanMove(facingDirection.Reverse(), world))
                {
                    directions.Add(facingDirection.Reverse(), 0.03f);
                }
            }

            return directions.Select(direction => new Move(direction.Key, direction.Value));
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap world)
        {
            return character.Skills
                .SelectMany(
                    skill => DirectionMethods.AllDirections
                        .Select(direction => new UseSkill(skill, direction))
                        .Where(move => move.Doable(character, world))
                )
                .Where(action => action.Doable(character, world));
        }
    }
}