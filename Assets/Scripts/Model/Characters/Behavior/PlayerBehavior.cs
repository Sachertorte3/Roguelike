using Cysharp.Threading.Tasks;
using Scripts.Model.Action;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private ActionReceiver _actionReceiver;
        public PlayerBehavior(ActionReceiver actionReceiver)
        {
            _actionReceiver = actionReceiver;
        }
        public UniTask<IAction> GenerateNextAction()
        {
            UniTask<IAction> action = _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.ReadInput();
            return action;
        }
    }
}
