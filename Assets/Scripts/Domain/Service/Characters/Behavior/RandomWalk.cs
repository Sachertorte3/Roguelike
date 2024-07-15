using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Service.Action;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class RandomWalk : IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world)
        {
            return GenerateMoveActionsDoable(character, world);
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character, IMap world)
        {
            return DirectionMethods.AllDirections.Where(direction => character.CanMove(direction, world))
                .Select(direction => new Move(direction));
        }
    }
}