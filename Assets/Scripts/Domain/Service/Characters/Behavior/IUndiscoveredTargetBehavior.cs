using System.Collections.Generic;
using Domain.Service.Action;

namespace Domain.Service.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world);
    }
}