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
        public UniTask<IAction> GenerateNextAction()
        {
            return _actionReceiver.ReceivedAction.WaitAsync();
        }
    }
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction()
        {
            return UniTask.FromResult<IAction>(new Move(Direction8.Right));
        }
    }
    public class ActionReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<IAction> ReceivedAction => _receivedAction;
        private AsyncReactiveProperty<IAction> _receivedAction = new AsyncReactiveProperty<IAction>(null);
        public void SetMoveAction(Direction8 direction)
        {
            _receivedAction.Value = new Move(direction);
        }
    }
}
