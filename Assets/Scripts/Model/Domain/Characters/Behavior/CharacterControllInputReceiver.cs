using System;
using Cysharp.Threading.Tasks;
using Model.Domain.Action;
using Model.Domain.Items;
using R3;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    public class CharacterControllInputReceiver
    {
        private readonly InventoryIndexReceiver _inventoryIndexReceiver = new();
        private readonly Subject<Unit> _onActionRead = new();
        private readonly AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));
        private readonly AsyncReactiveProperty<int> _onThrowItemActionReceived = new(0);
        private readonly AsyncReactiveProperty<int> _onUseItemActionReceived = new(0);

        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived =>
            _onMoveInputReceived;

        internal IReadOnlyAsyncReactiveProperty<int> OnUseItemActionReceived => _onUseItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<int> OnThrowItemActionReceived => _onThrowItemActionReceived;
        public Observable<Unit> OnActionRead => _onActionRead;

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