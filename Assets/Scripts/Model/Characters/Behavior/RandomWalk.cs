using Cysharp.Threading.Tasks;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.Characters.Behavior
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
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character)
        {
            return GenerateMoveActionsDoable(character).Cast<IAction>().Concat(GenerateUseSkillActionsDoable(character));
        }
        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character)
        {
            return DirectionMethods.AllDirections.Select(direction => new Move(direction)).Where(move => move.Doable(character));
        }
        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character)
        {
            return DirectionMethods.AllDirections.Select(direction => new UseSkill(new Skill(1, new LineArea(1)), direction)).Where(move => move.Doable(character));
        }
    }
}
