using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        private float behavioralRandomness = 0.02f;
        private IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private IDiscoveredTargetBehavior _chase = new Chase();
        private Vector2Int? _lastTargetPosition = null;
        public UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            HashSet<Vector2Int> visibleArea = character.Area.Get();
            visibleArea.Remove(character.CurrentPosition);
            HashSet<Character> visibleCharacters = GameManager.World.GetCharactersInArea(visibleArea);
            if (visibleCharacters.Any())
            {
                _lastTargetPosition = visibleCharacters.First().CurrentPosition;
            }
            else if (_lastTargetPosition.HasValue && (character.CurrentPosition == _lastTargetPosition
                || (!GameManager.World.Map.IsPassable(_lastTargetPosition.Value) && (character.CurrentPosition - _lastTargetPosition).Value.sqrMagnitude <= 2)))
            {
                _lastTargetPosition = null;
            }
            if (_lastTargetPosition.HasValue)
            {
                IEnumerable<IAction> actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value);
                return UniTask.FromResult(actions.MaxBy(action => action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
            }
            else
            {
                IEnumerable<IAction> actions = _wander.GenerateActionsDoable(character);
                return UniTask.FromResult(actions.MaxBy(action => action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
            }
        }
    }
}
