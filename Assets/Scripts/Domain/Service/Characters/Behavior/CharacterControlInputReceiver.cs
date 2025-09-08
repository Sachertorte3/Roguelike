using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Service.Action;
using R3;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    public class CharacterControlInputReceiver
    {
        private ItemFocus _focus = new(0);
        private readonly Subject<Unit> _onActionRead = new();
        private readonly AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));

        private readonly AsyncReactiveProperty<ItemFocus>
            _onUseItemActionReceived = new(new(0));

        private readonly AsyncReactiveProperty<ItemFocus> _onThrowItemActionReceived =
            new(new(0));

        private readonly AsyncReactiveProperty<ItemFocus> _onDropItemActionReceived =
            new(new(0));

        private readonly AsyncReactiveProperty<Unit> _onDoNothingActionReceived = new(Unit.Default);

        private readonly AsyncReactiveProperty<ItemFocus> _onRenameItemActionReceived =
            new(new(0));

        private readonly ReactiveProperty<bool> _enable = new(false);

        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived =>
            _onMoveInputReceived;

        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnUseItemActionReceived => _onUseItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnThrowItemActionReceived => _onThrowItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnDropItemActionReceived => _onDropItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<Unit> OnDoNothingActionReceived => _onDoNothingActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnRenameItemActionReceived => _onRenameItemActionReceived;
        public Observable<Unit> OnActionRead => _onActionRead;
        public ReadOnlyReactiveProperty<bool> IsEnabled => _enable;

        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            if (_enable.CurrentValue)
                _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }

        public void SetAttackInput()
        {
            if (_enable.CurrentValue)
                _onUseItemActionReceived.Value = _focus;
        }

        public void SetThrowInput()
        {
            if (_enable.CurrentValue)
                _onThrowItemActionReceived.Value = _focus;
        }

        public void SetDropInput()
        {
            if (_enable.CurrentValue)
                _onDropItemActionReceived.Value = _focus;
        }

        public void SetDoNothingInput()
        {
            if (_enable.CurrentValue)
                _onDoNothingActionReceived.Value = Unit.Default;
        }

        public void SetRenameInput()
        {
            if (_enable.CurrentValue)
                _onRenameItemActionReceived.Value = _focus;
        }

        public void SetItemFocus(ItemFocus focus)
        {
            if (_enable.CurrentValue)
                _focus = focus;
        }

        internal void ReadInput()
        {
            if (_enable.CurrentValue)
                _onActionRead.OnNext(Unit.Default);
        }

        public void Enable(bool enable)
        {
            _enable.Value = enable;
        }
    }
}