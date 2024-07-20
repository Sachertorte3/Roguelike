using System.Collections.Generic;
using Domain.Model.Action;

namespace Domain.Model.Character
{
    public interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap world);
    }
}