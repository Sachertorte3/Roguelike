using System.Collections.Generic;
using System.Linq;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class Escape : IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            var relativePosition = character.CurrentPosition - targetPosition;
            var directions = DirectionMethods.NearDirectionsFromVector(relativePosition);
            var moves = new List<Move> { new(directions[0], 0.02f), new(directions[1], 0.005f), new(directions[2], 0.005f) };
            return moves.Where(move => move.Doable(character, world));
        }
    }
}