using Assets.Scripts.Model.Items;
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
        internal IReadOnlyAsyncReactiveProperty<int> OnUseItemActionReceived => _onUseItemActionReceived;
        private AsyncReactiveProperty<int> _onUseItemActionReceived = new(0);
        internal IReadOnlyAsyncReactiveProperty<int> OnThrowItemActionReceived => _onThrowItemActionReceived;
        private AsyncReactiveProperty<int> _onThrowItemActionReceived = new(0);
        public Observable<Unit> OnActionRead => _onActionRead;
        private Subject<Unit> _onActionRead = new();
        private InventoryIndexReceiver _inventoryIndexReceiver = new();
        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }
        public void SetAttackInput()
        {
            _onUseItemActionReceived.Value = _inventoryIndexReceiver.Index;
        }
        public void SetThrowInput()
        {
            _onThrowItemActionReceived.Value = _inventoryIndexReceiver.Index;
        }
        public void SetInventoryIndex(int index)
        {
            _inventoryIndexReceiver.SetIndex(index);
        }
        internal void ReadInput()
        {
            _onActionRead.OnNext(Unit.Default);
        }
    }
}
