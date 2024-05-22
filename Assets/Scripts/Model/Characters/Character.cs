#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Data.Character.Type;
using Data.Condition;
using Model.Action;
using Model.Characters.Behavior;
using Model.Characters.Conditions;
using Model.Characters.Stats;
using Model.Effect;
using Model.Entities;
using Model.Items;
using Model.Setting;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Characters
{
    public sealed class Character : IDisposable, IActor, IHasBehavior, ITarget, IActorOfEffect, ITargetOfEffect, IHasCondition
    {
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Entity _entity;
        private readonly Inventory _inventory = new();
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        private readonly CharacterStats _stats;
        private readonly CharacterConditions _conditions;
        private readonly VisionRange _area;
        private bool _canIgnoreWall;
        internal bool CanAct = true;
        internal CharacterState State = CharacterState.Think;

        internal Character(Vector2Int position, ICharacterBehavior behavior, Observable<bool> canIgnoreWall)
        {
            CharacterType = new Human(Addressables
                .LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion());
            _entity = new Entity(position);
            Behavior = behavior;
            _stats = new CharacterStats(10, 2);
            _conditions = new CharacterConditions(this);
            _area = new VisionRange(_entity.Position);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<IEnumerable<Vector2Int>> OnSpawnEffect => _onSpawnEffect;

        public ICharacterType CharacterType { get; init; }
        private ICharacterBehavior Behavior { get; }
        public Entity Entity => _entity;
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;
        public ICharacterConditions Condition => _conditions;
        public int MaxHp => _stats.MaxHp.CurrentValue;

        /// <summary>
        ///     Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even
        ///     if the destination is impassable.
        ///     If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction)
        {
            return _canIgnoreWall
                ? Globals.World.IsPassableIgnoreWall(Position.CurrentValue + direction.Vector())
                : Globals.World.IsPassable(Position.CurrentValue + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (Globals.World.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) &&
                       Globals.World.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector())));
        }

        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }

        public async UniTask Move(Direction8 direction)
        {
            State = CharacterState.Act;
            Turn(direction);
            await _entity.Move(direction,
                Globals.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
            State = CharacterState.Wait;
        }

        public async UniTask UseSkill(Skill skill, Direction8 direction)
        {
            _direction.Value = direction;
            _onSpawnEffect.OnNext(skill.GetArea(CurrentPosition, CurrentDirection));
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await skill.Use(this, CurrentPosition, direction);
            State = CharacterState.Wait;
        }

        public async UniTask UseItem(int itemIndex, Direction8 direction)
        {
            Turn(direction);
            var item = _inventory.GetItem(itemIndex);
            if (item == null) throw new Exception("item is null");
            _onSpawnEffect.OnNext(item.Skill.GetArea(CurrentPosition, CurrentDirection));

            if (item.EffectsOnUse)
            {
                if (_entity.VisibleByPlayer.CurrentValue)
                    await UniTask.WhenAll(item.Use(this, CurrentPosition, direction),
                        UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
                else
                    await item.Use(this, CurrentPosition, direction);
            }
            State = CharacterState.Wait;
        }

        public async UniTask ThrowItem(int itemIndex, Direction8 direction)
        {
            Turn(direction);
            var item = _inventory.Remove(itemIndex);
            if (item == null) throw new Exception("item is null");
            var itemEntity = Globals.World.ActiveMap.CurrentValue.ItemManager.SpawnItem(item, CurrentPosition);
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(itemEntity.Throw(this, direction),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await itemEntity.Throw(this, direction);
            Globals.GameManager.LoadMap();
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
            _stats.Dispose();
            _conditions.Dispose();
        }

        public IVisionRange Area => _area;
        public IStats Stats => _stats;

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

        public async UniTask DoNextAction()
        {
            var action = await Behavior.GenerateNextAction(this);
            await action.Do(this);
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

        public UniTask GainHp(int value)
        {
            _stats.Hp.Gain(value);
            return UniTask.CompletedTask;
        }
        public UniTask LoseHp(int value)
        {
            _stats.Hp.Lose(value);
            return UniTask.CompletedTask;
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(condition, removalCondition);
        }

        public void UpdateTurn()
        {
            _conditions.UpdateTurn(this);
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