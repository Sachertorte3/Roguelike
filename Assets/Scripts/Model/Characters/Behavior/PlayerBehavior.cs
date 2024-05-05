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
        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            UniTask<IAction> actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.ReadInput();
            while (true)
            {
                IAction action = await actionTask;
                if (action.Doable(character))
                {
                    return action;
                }
                actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            }
        }
    }
}
