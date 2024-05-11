using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Action;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    public class CharacterControllInputReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived => _onMoveInputReceived;
        private AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));
        internal IReadOnlyAsyncReactiveProperty<int> OnItemActionReceived => _onSkillActionReceived;
        private AsyncReactiveProperty<int> _onSkillActionReceived = new(0);
        public Observable<Unit> OnActionRead => _onActionRead;
        private Subject<Unit> _onActionRead = new();
        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }
        public void SetAttackInput()
        {
            _onSkillActionReceived.Value = 0;
        }
        internal void ReadInput()
        {
            _onActionRead.OnNext(Unit.Default);
        }
    }
}
