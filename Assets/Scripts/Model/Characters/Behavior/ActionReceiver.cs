using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;
using System;
using UniRx;

namespace Scripts.Model.Characters.Behavior
{
    public class ActionReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<IAction> ReceivedAction => _receivedAction;
        private AsyncReactiveProperty<IAction> _receivedAction = new AsyncReactiveProperty<IAction>(null);
        public IObservable<Unit> OnWait => _onWait;
        private Subject<Unit> _onWait = new Subject<Unit>();
        public void SetMoveAction(Direction8 direction)
        {
            _receivedAction.Value = new Move(direction);
        }
        internal void ReadInput()
        {
            _onWait.OnNext(Unit.Default);
        }
    }
}
