using System.Collections.Generic;
using Model.Action;

namespace Model.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character);
    }
}