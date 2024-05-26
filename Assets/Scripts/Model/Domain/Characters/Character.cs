#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Data.Character.Type;
using Model.Action;
using Model.Characters.Behavior;
using Model.Domain;
using Model.Domain.Characters;
using Model.Effect;
using Model.Entities;
using Model.Items;
using Model.Logs;
using Model.Setting;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Characters
{
    public sealed class Character : IDisposable, IEntity, IActor, IHasBehavior, IActorOfEffect
    {
        private readonly string _name = "Character";
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Entity _entity;
        private readonly CharacterStatusManager _statusManager;
        private readonly Inventory _inventory = new();
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        private readonly VisionRange _area;
        private bool _canIgnoreWall;
        public bool CanAct = true;
        public CharacterState State = CharacterState.Think;
        private readonly CharacterAffiliationManager _affiliationManager;
        public IAffiliation Affiliation => _affiliationManager;

        internal Character(Vector2Int position, ICharacterBehavior behavior, Observable<bool> canIgnoreWall, IWorld world, CharacterGroup group)
        {
            CharacterType = new Human(Addressables
                .LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion());
            _entity = new(position);
            _statusManager = new(10, 2);
            Behavior = behavior;
            _area = new VisionRange(_entity.Position, world);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
            _affiliationManager = new CharacterAffiliationManager(group);
        }
        internal Character(EnemyData data, Vector2Int position, ICharacterBehavior behavior, Observable<bool> canIgnoreWall, IWorld world, CharacterGroup group)
        {
            CharacterType = data.CharacterType;
            _entity = new(position);
            _statusManager = new(data.Hp, data.Strength);
            Behavior = behavior;
            _area = new VisionRange(_entity.Position, world);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
            _affiliationManager = new CharacterAffiliationManager(group);
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<IEnumerable<Vector2Int>> OnSpawnEffect => _onSpawnEffect;
        public Observable<Unit> OnDead => _statusManager.OnDead;

        public ICharacterType CharacterType { get; init; }
        private ICharacterBehavior Behavior { get; }
        public Entity Entity => _entity;
        public IStatusManager StatusManager => _statusManager;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;

        /// <summary>
        ///     Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even
        ///     if the destination is not passable.
        ///     If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction, IWorld world)
        {
            return _canIgnoreWall
                ? world.IsMapPassable(Position.CurrentValue + direction.Vector())
                : world.IsPassable(Position.CurrentValue + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (world.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) &&
                       world.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector())));
        }

        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }

        public async UniTask Move(Direction8 direction, IInput input)
        {
            State = CharacterState.Act;
            Turn(direction);
            await _entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
            State = CharacterState.Wait;
        }

        public async UniTask UseSkill(Skill skill, Direction8 direction, IWorld world)
        {
            State = CharacterState.Act;
            _direction.Value = direction;
            _onSpawnEffect.OnNext(skill.GetArea(CurrentPosition, CurrentDirection));
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await skill.Use(this, CurrentPosition, direction, world);
            State = CharacterState.Wait;
        }

        public async UniTask UseItem(int itemIndex, Direction8 direction, IWorld world)
        {
            State = CharacterState.Act;
            Turn(direction);
            var item = _inventory.GetItem(itemIndex);
            if (item == null) throw new Exception("item is null");

            if (item.EffectsOnUse)
            {
                GameLog.Add($"{_name}:{item.Name}を使った");
                _onSpawnEffect.OnNext(item.Skill.GetArea(CurrentPosition, CurrentDirection));
                if (_entity.VisibleByPlayer.CurrentValue)
                    await UniTask.WhenAll(item.Use(this, CurrentPosition, direction, world),
                        UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
                else
                    await item.Use(this, CurrentPosition, direction, world);
            }
            State = CharacterState.Wait;
        }

        public async UniTask ThrowItem(int itemIndex, Direction8 direction, IWorld world)
        {
            State = CharacterState.Act;
            Turn(direction);
            var item = _inventory.Remove(itemIndex);
            if (item == null) throw new Exception("item is null");
            var itemEntity = world.SpawnItem(item, CurrentPosition);
            GameLog.Add($"{_name}:{item.Name}を投げた");
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(itemEntity.Throw(this, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await itemEntity.Throw(this, direction, world);
            State = CharacterState.Wait;
        }

        public void WasAttackedBy(IActorOfEffect actor)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(actor.CurrentPosition - CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }
        }

        public void Dispose()
        {
            _entity.Dispose();
            _inventory.Dispose();
            _onSpawnEffect.Dispose();
            _direction.Dispose();
        }

        public IVisionRange Area => _area;

        ~Character()
        {
            Dispose();
        }

        public string TypeName()
        {
            return CharacterType.TypeName();
        }

        public string SubtypeName()
        {
            return CharacterType.SubtypeName();
        }

        public async UniTask DoNextAction(IWorld world, IInput input)
        {
            var action = await Behavior.GenerateNextAction(this, world, input);
            await action.Do(this, world, input);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
            State = CharacterState.Wait;
        }

        public bool TryPickUp(Item item)
        {
            return _inventory.TryAdd(item);
        }

        public Item? ReplaceInventory(Item? item, int index)
        {
            return _inventory.Replace(item, index);
        }
        public void UpdateTurn()
        {
            _statusManager.UpdateTurn();
        }
    }
}
