using System.Collections.Generic;
using Domain.Model.Action;

namespace Domain.Model.Character
{
    public interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world);
    }
}