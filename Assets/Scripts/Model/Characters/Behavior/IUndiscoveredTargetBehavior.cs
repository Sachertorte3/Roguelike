using Scripts.Model.Action;
using System.Collections.Generic;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character);
    }
}
