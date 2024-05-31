using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Domain.Action;
using UnityEngine;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        private readonly IDiscoveredTargetBehavior _chase = new Chase();
        private readonly IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private readonly float behavioralRandomness = 0.01f;
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
                return UniTask.FromResult(actions.MaxBy(action =>
                    action.Evaluate(character, world) + Random.Range(0, behavioralRandomness)));
            }
            else
            {
                var actions = _wander.GenerateActionsDoable(character, world);
                return UniTask.FromResult(actions.MaxBy(action =>
                    action.Evaluate(character, world) + Random.Range(0, behavioralRandomness)));
            }
        }
    }
}