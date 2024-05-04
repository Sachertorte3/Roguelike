using Cysharp.Threading.Tasks;
using R3;
using Scripts.Data;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    public class ActionReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<IAction> ReceivedAction => _receivedAction;
        private AsyncReactiveProperty<IAction> _receivedAction = new AsyncReactiveProperty<IAction>(null);
        public Observable<Unit> OnWait => _onWait;
        private Subject<Unit> _onWait = new Subject<Unit>();
        public void SetMoveAction(Direction8 direction)
        {
            _receivedAction.Value = new Move(direction);
        }
        public void SetAttackAction()
        {
            _receivedAction.Value = new UseSkill(new Skill(10, new LineArea(1)));
        }
        internal void ReadInput()
        {
            _onWait.OnNext(Unit.Default);
        }
    }
}
