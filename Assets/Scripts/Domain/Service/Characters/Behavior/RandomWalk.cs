using System.Collections.Generic;
using System.Linq;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class RandomWalk : IBehaviorWhenUndiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap map)
        {
            return DirectionMethods.AllDirections.Where(direction => character.CanMove(direction, map))
                .Select(direction => new Move(direction));
        }
    }
}