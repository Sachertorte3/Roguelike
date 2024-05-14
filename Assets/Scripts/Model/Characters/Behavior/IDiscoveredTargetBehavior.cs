using System.Collections.Generic;
using Model.Action;
using UnityEngine;

namespace Model.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition);
    }
}