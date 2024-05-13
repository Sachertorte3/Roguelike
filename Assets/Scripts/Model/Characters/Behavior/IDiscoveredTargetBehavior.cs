using System.Collections.Generic;
using Scripts.Model.Action;
using UnityEngine;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition);
    }
}
