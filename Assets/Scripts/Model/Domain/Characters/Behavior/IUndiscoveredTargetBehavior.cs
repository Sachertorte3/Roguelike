using Model.Action;
using Model.Domain;
using System.Collections.Generic;

namespace Model.Characters.Behavior
{
    internal interface IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IWorld world);
    }
}