#nullable enable
using System;
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
    public sealed class Character : IDisposable, IActor, IHasBehavior, ITarget
    {
        private readonly Entity _entity;
        private readonly Inventory _inventory = new();
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
        internal bool CanAct = true;
        internal CharacterState State = CharacterState.Think;
        private readonly CharacterStats _stats;
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
        ~Character()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
            _inventory.Dispose();
            _onUseSkill.Dispose();
            _direction.Dispose();
            _stats.Dispose();
            _area.Dispose();
        }
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;

        public ICharacterType CharacterType { get; init; }
        public string TypeName() => CharacterType.TypeName();
        public string SubtypeName() => CharacterType.SubtypeName();
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;
        private ICharacterBehavior Behavior { get; init; }
        public IStats Stats => _stats;
        public IVisionRange Area => _area;
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
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
        public void SetVisiblity(bool visiblity) => _entity.SetVisibility(visiblity);
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
            await _entity.Move(direction, Globals.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
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
            if (_entity.VisibleByPlayer.CurrentValue)
            {
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            }
            else
            {
                await skill.Use(this, CurrentPosition, direction);
            }
            State = CharacterState.Wait;
        }
        public async UniTask UseItem(int itemIndex, Direction8 direction)
        {
            Turn(direction);
            Item? item = _inventory.GetItem(itemIndex);
            if (item == null)
            {
                throw new Exception("item is null");
            }
            _onUseSkill.OnNext((item.Skill, CurrentPosition, CurrentDirection));
            if (_entity.VisibleByPlayer.CurrentValue)
            {
                await UniTask.WhenAll(item.Use(this, CurrentPosition, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            }
            else
            {
                await item.Use(this, CurrentPosition, direction);
            }
            State = CharacterState.Wait;
        }
        public async UniTask ThrowItem(int itemIndex, Direction8 direction)
        {
            Turn(direction);
            Item? item = _inventory.Remove(itemIndex);
            if (item == null)
            {
                throw new Exception("item is null");
            }
            ItemEntity itemEntity = Globals.World.ItemManager.SpawnItem(item, CurrentPosition);
            if (_entity.VisibleByPlayer.CurrentValue)
            {
                await UniTask.WhenAll(itemEntity.Throw(this, direction), UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            }
            else
            {
                await itemEntity.Throw(this, direction);
            }
            State = CharacterState.Wait;
        }
        internal UniTask LoseHp(int value)
        {
            _stats.Hp.Lose(value);
            return UniTask.CompletedTask;
        }
        public bool TryPickUp(Item item)
        {
            return _inventory.TryAdd(item);
        }
        public Item? ReplaceInventory(Item? item, int index)
        {
            return _inventory.Replace(item, index);
        }
    }
}