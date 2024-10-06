using System.Collections.Generic;
using Domain.Model.Action;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map);
    }
}