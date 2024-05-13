using System.Collections.Generic;
using Scripts.Model.Action;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character);
    }
}
