using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Area;
using Model.Action;
using Model.Characters.Effect;
using UnityEngine;
using Utilities;

namespace Model.Characters.Behavior
{
    internal sealed class RandomWalk : IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character)
        {
            return GenerateMoveActionsDoable(character);
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character)
        {
            return DirectionMethods.AllDirections.Where(character.CanMove).Select(direction => new Move(direction));
        }
    }

    internal sealed class Chase : IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition)
        {
            return GenerateMoveActionsDoable(character, targetPosition).Cast<IAction>()
                .Concat(GenerateUseSkillActionsDoable(character));
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition)
        {
            var directions = DirectionMethods.NearDirectionFromVectors(targetPosition - character.CurrentPosition);
            return new List<Move> { new(directions[0], 0.1f), new(directions[1], 0.05f), new(directions[2], 0.01f) }
                .Where(move => move.Doable(character));
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character)
        {
            return DirectionMethods.AllDirections
                .Select(direction => new UseSkill(new Skill(new SkillData(new LineArea(1, false), new AttackEffect(1))), direction))
                .Where(move => move.Doable(character));
        }
    }
}