using Cysharp.Threading.Tasks;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    public class ActionReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<(IAction action, bool isStarted)> ReceivedAction => _receivedAction;
        private AsyncReactiveProperty<(IAction action, bool isStarted)> _receivedAction = new AsyncReactiveProperty<(IAction action, bool isStarted)>((null, false));
        public Observable<Unit> OnActionRead => _onActionRead;
        private Subject<Unit> _onActionRead = new Subject<Unit>();
        public void SetMoveAction(Direction8 direction, bool isStarted)
        {
            _receivedAction.Value = (new Move(direction), isStarted);
        }
        public void SetAttackAction()
        {
            _receivedAction.Value = (new UseSkill(new Skill(10, new LineArea(1)), Direction8.Up), true);
        }
        internal void ReadInput()
        {
            _onActionRead.OnNext(Unit.Default);
        }
    }
}
