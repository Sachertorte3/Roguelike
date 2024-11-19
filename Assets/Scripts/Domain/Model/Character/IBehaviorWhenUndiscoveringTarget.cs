using System.Collections.Generic;
using Domain.Model.Map;

namespace Domain.Model.Character
{
    public interface IBehaviorWhenUndiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap map);
    }
}