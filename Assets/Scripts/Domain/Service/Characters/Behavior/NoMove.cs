using System.Collections.Generic;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    public sealed class NoMove : IBehaviorWhenUndiscoveringTarget, IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap map)
        {
            return new List<IAction> { new DoNothing() };
        }

        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            return GenerateMoveActionsDoable(character, map);
        }
    }
}