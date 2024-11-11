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

        private readonly AsyncReactiveProperty<ItemFocus>
            _onUseItemActionReceived = new(new ItemFocus(0, false, false));

        private readonly AsyncReactiveProperty<ItemFocus> _onThrowItemActionReceived =
            new(new ItemFocus(0, false, false));

        private readonly AsyncReactiveProperty<ItemFocus> _onDropItemActionReceived =
            new(new ItemFocus(0, false, false));

        private readonly AsyncReactiveProperty<Unit> _onDoNothingActionReceived = new(Unit.Default);

        private readonly AsyncReactiveProperty<ItemFocus> _onRenameItemActionReceived =
            new(new ItemFocus(0, false, false));

        private bool _enable = true;

        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived =>
            _onMoveInputReceived;

        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnUseItemActionReceived => _onUseItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnThrowItemActionReceived => _onThrowItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnDropItemActionReceived => _onDropItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<Unit> OnDoNothingActionReceived => _onDoNothingActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnRenameItemActionReceived => _onRenameItemActionReceived;
        public Observable<Unit> OnActionRead => _onActionRead;

        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            if (_enable)
                _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }

        public void SetAttackInput()
        {
            if (_enable)
                _onUseItemActionReceived.Value = _inventoryIndexReceiver.Focus;
        }

        public void SetThrowInput()
        {
            if (_enable)
                _onThrowItemActionReceived.Value = _inventoryIndexReceiver.Focus;
        }

        public void SetDropInput()
        {
            if (_enable)
                _onDropItemActionReceived.Value = _inventoryIndexReceiver.Focus;
        }

        public void SetDoNothingInput()
        {
            if (_enable)
                _onDoNothingActionReceived.Value = Unit.Default;
        }

        public void SetRenameInput()
        {
            if (_enable)
                _onRenameItemActionReceived.Value = _inventoryIndexReceiver.Focus;
        }

        public void SetItemFocus(ItemFocus focus)
        {
            if (_enable)
                _inventoryIndexReceiver.SetFocus(focus);
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