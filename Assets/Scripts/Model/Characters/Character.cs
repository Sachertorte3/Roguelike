using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Utilities;
using System;
using UniRx;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public sealed class Character : IActor
    {
        public IReadOnlyReactiveProperty<Vector2Int> Position => _position;
        private readonly ReactiveProperty<Vector2Int> _position;
        public IObservable<Direction8> OnMove => _onMove;
        private readonly Subject<Direction8> _onMove = new Subject<Direction8>();
        internal bool CanAct = true;
        internal CharacterState State = CharacterState.Think;
        internal ICharacterBehavior Behavior => _behavior;
        private ICharacterBehavior _behavior;
        internal Character(Vector2Int position, ICharacterBehavior behavior)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
            _behavior = behavior;
        }
        public async UniTask DoNextAction()
        {
            IAction action = await _behavior.GenerateNextAction();
            await action.Do(this);
        }
        public void Move(Direction8 direction)
        {
            if (State != CharacterState.Think)
            {
                return;
            }
            _onMove.OnNext(direction);
            _position.Value += direction.Vector();
            State = CharacterState.Wait;
        }
        public void Teleport(Vector2Int position)
        {

        }
    }
}