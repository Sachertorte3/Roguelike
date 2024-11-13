using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    internal static class NoMove
    {
        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap map)
        {
            return new List<IAction> { new DoNothing() };
        }

        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            return GenerateMoveActionsDoable(character, map);
        }
    }
}