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
        private float behavioralRandomness = 0.2f;
        private IWanderBehavior _wander = new RandomWalk();
        public UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            IEnumerable<IAction> actions = _wander.GenerateMoveActionsDoable(character);
            return UniTask.FromResult(actions.Where(action => action.Doable(character)).MaxBy(action => action.Evaluate(character) + Random.Range(0, behavioralRandomness)));
        }
    }
}
