using Assets.Scripts.Model.Items;
using Cysharp.Threading.Tasks;
using Database.Characters.Type;
using R3;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Characters.Stats;
using Scripts.Model.Entities;
using Scripts.Model.Items;
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
        private readonly Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public IInventory Inventory => _inventory;
        private readonly Inventory _inventory = new();
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public ReactiveProperty<Direction8> Direction => _direction;
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        internal bool CanAct = true;
        internal bool VisibleByPlayer = false;
        internal CharacterState State = CharacterState.Think;
        private ICharacterBehavior Behavior { get; init; }
        public IStats Stats => _stats;
        private readonly CharacterStats _stats;
        public IVisionRange Area => _area;
        private readonly VisionRange _area;
        private bool _canIgnoreWall;
        internal Character(Vector2Int position, ICharacterBehavior behavior, Observable<bool> canIgnoreWall)
        {
            CharacterType = new Human(Addressables.LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion());
            _entity = new Entity(position);
            Behavior = behavior;
            _stats = new CharacterStats(10, 2);
            _area = new VisionRange(_entity.Position);
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
        public async UniTask UseItem(Item item, Direction8 direction)
        {
            _direction.Value = direction;
            _onUseSkill.OnNext((item.Skill, CurrentPosition, CurrentDirection));
            if (VisibleByPlayer)
            {
                await UniTask.WhenAll(item.Use(this, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            }
            State = CharacterState.Wait;
        }
        internal UniTask LoseHp(int value)
        {
            _stats.Hp.Lose(value);
            return UniTask.CompletedTask;
        }
        internal bool HasEmptySpaceInInventory() => Inventory.HasEmptySpace();
        internal bool TryPickUp(Item item)
        {
            return _inventory.TryAdd(item);
        }
    }
}