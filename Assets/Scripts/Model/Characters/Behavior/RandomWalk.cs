using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class RandomWalk : IWanderBehavior
    {
        public IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character)
        {
            return DirectionMethods.AllDirections.Where(character.CanMove).Select(direction => new Move(direction));
        }
    }
}
