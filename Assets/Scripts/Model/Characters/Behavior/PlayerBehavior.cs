using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private ActionReceiver _actionReceiver;
        public PlayerBehavior(ActionReceiver actionReceiver)
        {
            _actionReceiver = actionReceiver;
        }
        public async UniTask<IAction> GenerateNextAction()
        {
            _actionReceiver.waiting = true;
            IAction action = await _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.waiting = false;
            return action;
        }
    }
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction()
        {
            return UniTask.FromResult<IAction>(new Move(Direction8.Right));
        }
    }
}
