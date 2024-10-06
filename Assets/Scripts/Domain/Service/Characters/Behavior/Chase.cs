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
    public class MoveCostCalculator
    {
        private IHasBehavior _character;
        private IMap _map;
        private bool _canSwap;
        public MoveCostCalculator(IHasBehavior character, IMap map, bool canSwap)
        {
            _character = character;
            _map = map;
            _canSwap = canSwap;
        }

        public float Calculate(Vector2Int pos, Direction8 direction)
        {
            if (_character.CanMove(pos, direction, _map))
                return 1;
            if (_canSwap && _character.CanSwap(pos, direction, _map))
                return 1 + 0.01f;
            return float.PositiveInfinity;
        }
    }

    internal sealed class Chase : IBehaviorWhenDiscoveringTarget
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            var calculator = new MoveCostCalculator(character, map, false);
            var route = new AStar(calculator.Calculate).Calc(character.CurrentPosition, targetPosition);
            if (route.Count < 2)
            {
                Log.Debug("Already reached the target position");
                return Enumerable.Empty<Move>();
            }

            var direction = DirectionMethods.FromVector(route[1] - route[0]);
            foreach (var pos in route)
            {
                Log.Debug($"pos: {pos}");
            }

            var move = new Move(direction!.Value, 0.01f);
            var swap = new Swap(direction!.Value, 0.01f);
            if (move.Doable(character, map))
            {
                return new List<Move> { move };
            }

            if (swap.Doable(character, map))
            {
                return new List<Swap> { swap };
            }

            Log.Debug($"Move to {direction} is not doable");
            return Enumerable.Empty<Move>();
        }
    }
}