using System.Collections.Generic;
using Domain.Model.Action;
using Domain.Service;

namespace Domain.Model.Characters
{
    public interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world);
    }
}