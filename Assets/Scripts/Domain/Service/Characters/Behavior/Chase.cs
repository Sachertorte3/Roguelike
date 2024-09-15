using System.Collections.Generic;
using System.Linq;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class Chase : IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            var route = new AStar(world.GetAllPassablePositions()).Calc(character.CurrentPosition, targetPosition);
            if (route.Count < 2)
            {
                Log.Debug($"Already reached the target position");
                return Enumerable.Empty<Move>();
            }

            var direction = DirectionMethods.FromVector(route[1] - route[0]);

            var move = new Move(direction, 0.01f);
            if (move.Doable(character, world))
            {
                return new List<Move> { move };
            }

            Log.Debug($"Move to {direction} is not doable");
            return Enumerable.Empty<Move>();
        }
    }
}