using System.Collections.Generic;
using Domain.Service.Action;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world);
    }
}