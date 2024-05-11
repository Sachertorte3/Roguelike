using Cysharp.Threading.Tasks;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Items;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    public class CharacterControllInputReceiver
    {
        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived => _onMoveInputReceived;
        private AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));
        internal IReadOnlyAsyncReactiveProperty<Item> OnItemActionReceived => _onSkillActionReceived;
        private AsyncReactiveProperty<Item> _onSkillActionReceived = new(null);
        public Observable<Unit> OnActionRead => _onActionRead;
        private Subject<Unit> _onActionRead = new Subject<Unit>();
        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }
        public void SetAttackInput()
        {
            _onSkillActionReceived.Value = new Item(new Skill(10, new LineArea(1)), 1);
        }
        internal void ReadInput()
        {
            _onActionRead.OnNext(Unit.Default);
        }
    }
}
