using Cysharp.Threading.Tasks;
using Domain.Service.Action;
using Domain.Service.Items;
using R3;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    public class CharacterControlInputReceiver
    {
        private readonly InventoryIndexReceiver _inventoryIndexReceiver = new();
        private readonly Subject<Unit> _onActionRead = new();
        private readonly AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));
        private readonly AsyncReactiveProperty<int?> _onThrowItemActionReceived = new(0);
        private readonly AsyncReactiveProperty<int?> _onUseItemActionReceived = new(0);
        private bool _enable = true;

        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived =>
            _onMoveInputReceived;

        internal IReadOnlyAsyncReactiveProperty<int?> OnUseItemActionReceived => _onUseItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<int?> OnThrowItemActionReceived => _onThrowItemActionReceived;
        public Observable<Unit> OnActionRead => _onActionRead;

        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            if (_enable)
                _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }

        public void SetAttackInput()
        {
            if (_enable)
                _onUseItemActionReceived.Value = _inventoryIndexReceiver.Index;
        }

        public void SetThrowInput()
        {
            if (_enable)
                _onThrowItemActionReceived.Value = _inventoryIndexReceiver.Index;
        }

        public void SetInventoryIndex(int? index)
        {
            if (_enable)
                _inventoryIndexReceiver.SetIndex(index);
        }

        internal void ReadInput()
        {
            if (_enable)
                _onActionRead.OnNext(Unit.Default);
        }

        public void Enable(bool enable)
        {
            _enable = enable;
        }
    }
}