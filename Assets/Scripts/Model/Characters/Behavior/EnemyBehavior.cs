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
        private float behavioralRandomness = 0.1f;
        private IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private IDiscoveredTargetBehavior _chase = new Chase();
        public UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            HashSet<Vector2Int> visibleArea = character.Area.Get();
            visibleArea.Remove(character.CurrentPosition);
            HashSet<Character> visibleCharacters = GameManager.World.GetCharactersInArea(visibleArea);
            if (visibleCharacters.Any())
            {
                Debug.Log($"visible! {visibleCharacters.First().CurrentPosition}");
                IEnumerable<IAction> actions = _chase.GenerateActionsDoable(character, visibleCharacters.First().CurrentPosition);
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
