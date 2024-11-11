using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

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
                return 2;
            return float.PositiveInfinity;
        }
    }
}