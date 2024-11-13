using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal static class Chase
    {
        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            var calculator = new MoveCostCalculator(character, map, true);
            var route = new AStar(calculator.Calculate).Calc(character.Entity.CurrentPosition, targetPosition);
            if (route.Count < 2)
            {
                Log.Debug("[Think]Already reached the target position");
                return Enumerable.Empty<Move>();
            }

            var direction = DirectionMethods.FromVector(route[1] - route[0]);

            var move = new Move(direction!.Value, 0.01f);
            var swap = new Swap(direction!.Value, 0.01f);
            if (move.Doable(character, map))
            {
                return new List<Move> { move };
            }

            if (swap.Doable(character, map))
            {
                //return new List<Swap> { swap };
            }

            Log.Debug($"[Think]Move to {direction} is not doable");
            return Enumerable.Empty<Move>();
        }
    }
}