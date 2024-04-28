using Scripts.Utilities;
using System;
using UniRx;
using UnityEngine;

namespace Scripts.Model
{
    public sealed class Character
    {
        private IReadOnlyReactiveProperty<Vector2Int> Position => _position;
        public ReactiveProperty<Vector2Int> _position = new ReactiveProperty<Vector2Int>();
        public IObservable<Direction8> MoveSubject => _moveSubject;
        private readonly Subject<Direction8> _moveSubject = new Subject<Direction8>();
        public bool CanAct = true;
        public CharacterState State = CharacterState.Think;
        public void Move(Direction8 direction)
        {
            if (State != CharacterState.Think)
            {
                return;
            }
            _moveSubject.OnNext(direction);
            _position.Value += direction.Vector();
            State = CharacterState.Wait;
        }
        public void Teleport(Vector2Int position)
        {

        }
    }
    public enum CharacterState
    {
        Think,
        Act,
        Wait,
    }
}