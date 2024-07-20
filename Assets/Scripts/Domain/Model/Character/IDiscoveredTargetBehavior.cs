using System.Collections.Generic;
using Domain.Model.Action;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world);
    }
}