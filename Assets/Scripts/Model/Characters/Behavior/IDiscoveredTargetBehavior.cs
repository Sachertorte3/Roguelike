using Scripts.Model.Action;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition);
    }
}
