using Scripts.Model.Action;
using System.Collections.Generic;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IWanderBehavior
    {
        public IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character);
    }
}
