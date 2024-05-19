using Cysharp.Threading.Tasks;
using Model.Action;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Model.Characters.Behavior
{
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        private readonly IDiscoveredTargetBehavior _chase = new Chase();
        private Vector2Int? _lastTargetPosition;
        private readonly IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private readonly float behavioralRandomness = 0.02f;

        public UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            var visibleArea = character.Area.Get();
            visibleArea.Remove(character.CurrentPosition);
            var visibleCharacters = Globals.World.ActiveMap.CurrentValue.GetCharactersInArea(visibleArea);
            if (visibleCharacters.Any())
                _lastTargetPosition = visibleCharacters.First().CurrentPosition;
            else if (_lastTargetPosition.HasValue && (character.CurrentPosition == _lastTargetPosition
                                                      || (!Globals.World.ActiveMap.CurrentValue.Tilemap.IsPassable(_lastTargetPosition.Value) &&
                                                          (character.CurrentPosition - _lastTargetPosition).Value
                                                          .sqrMagnitude <= 2)))
                _lastTargetPosition = null;
            if (_lastTargetPosition.HasValue)
            {
                var actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value);
                return UniTask.FromResult(actions.MaxBy(action =>
                    action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
            }
            else
            {
                var actions = _wander.GenerateActionsDoable(character);
                return UniTask.FromResult(actions.MaxBy(action =>
                    action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
            }
        }
    }
}