using System.Collections.Generic;
using System.Linq;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class Wander : IBehaviorWhenUndiscoveringTarget, IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap world)
        {
            var directions = new Dictionary<Direction8, float>();
            var facingDirection = character.CurrentDirection;
            if (character.CanMove(facingDirection, world))
            {
                directions.Add(facingDirection, 0.1f);
                if (!world.IsWalkableOnMap(character.CurrentPosition +
                                         facingDirection.RotateClockwise(new Angle(135)).Vector())
                    && character.CanMove(facingDirection.RotateClockwise(new Angle(90)), world))
                {
                    directions.Add(facingDirection.RotateClockwise(new Angle(90)), 0.1f);
                }

                if (!world.IsWalkableOnMap(character.CurrentPosition +
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

        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition, IMap world)
        {
            return GenerateMoveActionsDoable(character, world);
        }
    }
}