using System.Collections.Generic;
using Model.Domain.Action;
using UnityEngine;

namespace Model.Domain.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world);
    }
}