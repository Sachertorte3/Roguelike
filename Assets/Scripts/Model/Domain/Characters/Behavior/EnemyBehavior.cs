using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Domain.Action;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        private readonly IDiscoveredTargetBehavior _chase = new Chase();
        private readonly IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private readonly float behavioralRandomness = 0.0f;
        private Vector2Int? _lastTargetPosition;

        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            HashSet<Vector2Int> visibleArea = new(character.Area.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);
            var visibleCharacters = world.GetCharactersInArea(visibleArea);
            if (visibleCharacters.Any())
                _lastTargetPosition = visibleCharacters.First().CurrentPosition;
            else if (_lastTargetPosition.HasValue && (character.CurrentPosition == _lastTargetPosition
                                                      || !world.IsReachable(character.CurrentPosition,
                                                          _lastTargetPosition.Value)))
                _lastTargetPosition = null;
            if (_lastTargetPosition.HasValue)
            {
                var actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value, world);
                var validActions = actions.Where(action => action.Evaluate(character, world) > 0).ToList();
                return UniTask.FromResult(validActions.MaxByOrDefault(action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness), new DoNothing()));
            }
            else
            {
                var actions = _wander.GenerateActionsDoable(character, world);
                var validActions = actions.Where(action => action.Evaluate(character, world) > 0).ToList();
                return UniTask.FromResult(validActions.MaxByOrDefault(action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness), new DoNothing()));
            }
        }
    }
}