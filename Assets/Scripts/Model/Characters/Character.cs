using Cysharp.Threading.Tasks;
using Database.Characters.Type;
using R3;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Characters.Stats;
using Scripts.Model.Entities;
using Scripts.Model.Setting;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Model.Characters
{
    public sealed class Character : IActor, IHasBehavior, ITarget
    {
        public ICharacterType CharacterType { get; init; }
        public string TypeName() => CharacterType.TypeName();
        public string SubtypeName() => CharacterType.SubtypeName();
        private Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _onMove;
        private readonly Subject<(Direction8 direction, Vector2Int destination)> _onMove = new();
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
        public bool IsDead => Stats.Hp.Value.CurrentValue <= 0;
        public Observable<Unit> OnDead => Stats.Hp.Value.Where(value => value <= 0).AsUnitObservable();
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public ReactiveProperty<Direction8> Direction => _direction;
        private ReactiveProperty<Direction8> _direction = new ReactiveProperty<Direction8>(Direction8.Down);
        internal bool CanAct = true;
        public bool VisibleByPlayer = false;
        internal CharacterState State = CharacterState.Think;
        internal ICharacterBehavior Behavior { get; init; }
        public CharacterStats Stats { get; init; }
        public VisionRange Area { get; init; }
        private bool _canIgnoreWall;
        internal Character(Vector2Int position, ICharacterBehavior behavior, Observable<bool> canIgnoreWall)
        {
            CharacterType = new Human(Addressables.LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion());
            _entity = new Entity(position);
            Behavior = behavior;
            Stats = new CharacterStats(10, 2);
            Area = new VisionRange(_entity.Position);
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
            return _canIgnoreWall ?
                Globals.World.IsPassableIgnoreWall(Position.CurrentValue + direction.Vector()) :
                (Globals.World.IsPassable(Position.CurrentValue + direction.Vector())
                && (!direction.IsDiagonal() || (Globals.World.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) && Globals.World.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector()))));
        }
        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }
        public async UniTask Move(Direction8 direction)
        {
            State = CharacterState.Act;
            if (!CanMove(direction))
            {
                return;
            }
            Turn(direction);
            _entity.Move(direction);
            _onMove.OnNext((direction, _entity.CurrentPosition));
            if (VisibleByPlayer)
            {
                await UniTask.Delay(Globals.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
            }
            State = CharacterState.Wait;
        }
        public void Teleport(Vector2Int position)
        {

            State = CharacterState.Wait;
        }
        public async UniTask UseSkill(Skill skill, Direction8 direction)
        {
            _direction.Value = direction;
            _onUseSkill.OnNext((skill, CurrentPosition, CurrentDirection));
            if (VisibleByPlayer)
            {
                await UniTask.WhenAll(skill.Use(this, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            }
            State = CharacterState.Wait;
        }
    }
}