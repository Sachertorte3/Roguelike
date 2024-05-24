using Model.Action;
using Model.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Characters.Behavior
{
    internal interface IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition, IWorld world);
    }
}