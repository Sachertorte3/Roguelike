using System.Collections.Generic;
using Model.Domain.Action;

namespace Model.Domain.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world);
    }
}