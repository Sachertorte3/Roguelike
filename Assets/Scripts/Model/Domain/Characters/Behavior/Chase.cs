using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Area;
using Model.Domain.Action;
using Model.Domain.Effect;
using UnityEngine;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    internal sealed class Chase : IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            return GenerateMoveActionsDoable(character, targetPosition, world).Cast<IAction>()
                .Concat(GenerateUseSkillActionsDoable(character, world));
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            var directions = DirectionMethods.NearDirectionsFromVector(targetPosition - character.CurrentPosition);
            return new List<Move> { new(directions[0], 0.1f), new(directions[1], 0.05f), new(directions[2], 0.01f) }
                .Where(move => move.Doable(character, world));
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap world)
        {
            return DirectionMethods.AllDirections
                .Select(direction => new UseSkill(new Skill(new SkillData(new LineArea(1, false), new AttackEffect(1))),
                    direction))
                .Where(move => move.Doable(character, world));
        }
    }
}