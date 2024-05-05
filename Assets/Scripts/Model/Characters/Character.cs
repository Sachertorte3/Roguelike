using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Characters.Stats;
using Scripts.Model.Setting;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public sealed class Character : IActor, IHasBehavior, ITarget
    {
        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        private readonly ReactiveProperty<Vector2Int> _position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
        public Observable<Unit> OnDead => Stats.Hp.Value.Where(value => value <= 0).AsUnitObservable();
        public Direction8 CurrentDirection { get; private set; }
        internal bool CanAct = true;
        internal CharacterState State = CharacterState.Think;
        internal ICharacterBehavior Behavior { get; init; }
        public CharacterStats Stats { get; init; }
        public VisibleArea Area { get; init; }
        private bool _canIgnoreWall;
        internal Character(Vector2Int position, ICharacterBehavior behavior, ReactiveProperty<bool> canIgnoreWall)
        {
            _position = new ReactiveProperty<Vector2Int>(position);
            Behavior = behavior;
            Stats = new CharacterStats(10, 2);
            Area = new VisibleArea(_position);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
        }
        public async UniTask DoNextAction()
        {
            IAction action = await Behavior.GenerateNextAction(this);
            await action.Do(this);
        }
        /// <summary>
        /// Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even if the destination is impassable.
        /// If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction)
        {
            return _canIgnoreWall?
                GameManager.World.IsPassableIgnoreWall(Position.CurrentValue + direction.Vector()): 
                (GameManager.World.IsPassable(Position.CurrentValue + direction.Vector())
                && (!direction.IsDiagonal() || (GameManager.World.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) && GameManager.World.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector()))));
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
            CurrentDirection = direction;
            _onMove.OnNext((direction, CurrentPosition));
            if (!GameManager.IsDash())
            {
                await UniTask.Delay(Settings.MoveMilliseconds.Value);
            }
            State = CharacterState.Wait;
        }
        public void Teleport(Vector2Int position)
        {

        }
        public async UniTask UseSkill(Skill skill, Direction8 direction)
        {
            CurrentDirection = direction;
            _onUseSkill.OnNext((skill, CurrentPosition, CurrentDirection));
            await UniTask.WhenAll(skill.Use(this, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            State = CharacterState.Wait;
        }
    }
}