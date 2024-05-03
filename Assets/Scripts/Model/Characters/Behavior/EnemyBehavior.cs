using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;
using System.Collections;
using System.ComponentModel;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        private IWanderBehavior _wander = new RandomWalk();
        public UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            return UniTask.FromResult<IAction>(_wander.GenerateMoveActionsDoable(character).GetAtRandom());
        }
    }
}
