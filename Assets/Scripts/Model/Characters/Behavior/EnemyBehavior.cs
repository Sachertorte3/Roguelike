using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;
using System.Collections.Generic;
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
            IEnumerable<IAction> actions = _wander.GenerateActionsDoable(character);
            return UniTask.FromResult(actions.MaxBy(action => action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
        }
    }
}
