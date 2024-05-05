using Cysharp.Threading.Tasks;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    public class CharacterControllInputReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived => _onMoveInputReceived;
        private AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new AsyncReactiveProperty<(Move action, bool isStarted)>((null, false));
        internal IReadOnlyAsyncReactiveProperty<Skill> OnSkillActionReceived => _onSkillActionReceived;
        private AsyncReactiveProperty<Skill> _onSkillActionReceived = new AsyncReactiveProperty<Skill>(null);
        public Observable<Unit> OnActionRead => _onActionRead;
        private Subject<Unit> _onActionRead = new Subject<Unit>();
        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }
        public void SetAttackInput()
        {
            _onSkillActionReceived.Value = new Skill(10, new LineArea(1));
        }
        internal void ReadInput()
        {
            _onActionRead.OnNext(Unit.Default);
        }
    }
}
