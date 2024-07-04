using System.Collections.Generic;
using Domain.Model.Action;
using Domain.Service;
using UnityEngine;

namespace Domain.Model.Characters
{
    public interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world);
    }
}