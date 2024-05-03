using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Setting;
using Scripts.Utilities;
using System;
using UniRx;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public sealed class Character : IActor, IHasBehavior
    {
        public IReadOnlyReactiveProperty<Vector2Int> Position => _position;
        private readonly ReactiveProperty<Vector2Int> _position;
        public IObservable<Direction8> OnMove => _onMove;
        private readonly Subject<Direction8> _onMove = new Subject<Direction8>();
        internal bool CanAct = true;
        internal CharacterState State = CharacterState.Think;
        internal ICharacterBehavior Behavior => _behavior;
        private ICharacterBehavior _behavior;
        private World _world;
        internal Character(Vector2Int position, ICharacterBehavior behavior, World world)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
            _behavior = behavior;
            _world = world;
        }
        public async UniTask DoNextAction()
        {
            IAction action = await _behavior.GenerateNextAction(this);
            await action.Do(this);
        }
        /// <summary>
        /// Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even if the destination is impassable.
        /// If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction)
        {
            return Settings.IgnoreWall.Value? _world.IsPassableIgnoreWall(Position.Value + direction.Vector()): _world.IsPassable(Position.Value + direction.Vector());
        }
        public async UniTask Move(Direction8 direction)
        {
            State = CharacterState.Act;
            if (!CanMove(direction))
            {
                State = CharacterState.Wait;
                return;
            }
            _position.Value += direction.Vector();
            _onMove.OnNext(direction);
            await UniTask.Delay(Settings.MoveMilliseconds.Value);
            State = CharacterState.Wait;
        }
        public void Teleport(Vector2Int position)
        {

        }
    }
}