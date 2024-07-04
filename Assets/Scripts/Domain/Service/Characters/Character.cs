#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Area;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Setting;
using Effect.Position;
using Domain.Service.Action;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Entities;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using AdditionalConditionData = Domain.Model.AdditionalConditionData;
using Domain.Model.Action;
using Domain.Model.Characters;
using Domain.Model.Message;
using Domain.Model.Items;

namespace Domain.Service.Characters
{
    internal sealed class Character : ICharacter
    {
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly Aggression _aggression;
        private readonly VisionRange _area;
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Entity _entity;
        private readonly Inventory _inventory;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        private readonly Subject<Unit> _onPickUpItem = new();
        private readonly ISkill[] _skills;
        private readonly CharacterStatusManager _statusManager;
        public bool IsLeader { get; init; }
        public bool IsBoss { get; init; }
        private bool _canIgnoreWall;
        private string _name = "Character";
        public CharacterState State { get; set; } = CharacterState.Think;
        private int _money = 120;
        public int Money => _money;

        internal Character(CharacterMemento data, ICharacterBehavior behavior, Observable<bool> canIgnoreWall,
            IMap world)
        {
            _name = data.Name;
            CharacterType = data.CharacterType;
            _entity = new Entity(data.EntityData);
            _skills = data.Skills.Select(x => new Skill(x)).ToArray();
            _inventory = new Inventory(data.Inventory);
            _statusManager = new CharacterStatusManager(data.Name, data.Status);
            Behavior = behavior;
            _area = new VisionRange(_entity.Position, world);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
            _affiliationManager = new CharacterAffiliationManager(data.Affiliation);
            _aggression = data.Aggression;
            IsLeader = data.IsLeader;
            IsBoss = data.IsBoss;
        }

        public string Name => _name;
        private bool _canAct => _statusManager.Conditions.All(condition => condition.CanAct);
        private bool _isConfused => _statusManager.Conditions.Any(condition => condition.CausesConfusion);

        public bool CanAct => _canAct;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;
        public Observable<Unit> OnDead => _statusManager.OnDead;
        public Observable<Unit> OnPickUpItem => _onPickUpItem;
        public ICharacterType CharacterType { get; init; }
        private ICharacterBehavior Behavior { get; }
        public IStatusManager StatusManager => _statusManager;
        public Aggression Aggression => _aggression;
        public IAffiliation Affiliation => _affiliationManager;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;

        /// <summary>
        ///     Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even
        ///     if the destination is not passable.
        ///     If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction, IPassableChecker world)
        {
            return _canIgnoreWall
                ? true
                : world.IsPassable(Position.CurrentValue + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (world.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) &&
                       world.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector())));
        }

        public bool CanMoveIgnoreCharacter(Direction8 direction, IPassableChecker world)
        {
            return _canIgnoreWall
                ? true
                : world.IsMapPassable(Position.CurrentValue + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (world.IsMapPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) &&
                       world.IsMapPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector())));
        }

        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }

        public void DoNothing()
        {
            Log.Debug($"[Action]{_name}:DoNothing");
            State = CharacterState.Wait;
        }

        public async UniTask Move(Direction8 direction, IInput input)
        {
            Log.Debug($"[Action]{_name}:Move direction:{direction}");
            Turn(direction);
            await _entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);

            State = CharacterState.Wait;
        }

        public async UniTask UseSkill(ISkill skill, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseSkill\n{skill.Info()}\ndirection:{direction}");
            Turn(direction);
            _onEffectSpawned.OnNext(
                new OnEffectSpawnedMessage(skill.GetArea(this, CurrentPosition, CurrentDirection, map), skill.Color));
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction, map),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await skill.Use(this, CurrentPosition, direction, map);

            State = CharacterState.Wait;
        }

        public async UniTask UseItem(int itemIndex, Direction8 direction, IMap map)
        {
            var item = _inventory.GetItem(itemIndex);
            if (item == null) throw new Exception("item is null");
            Log.Debug($"[Action]{_name}:UseItem\n{item.Info()}\ndirection:{direction}");
            Turn(direction);

            if (item.EffectsOnUse)
            {
                GameLog.Add($"{_name}:{item.Name}を使った");
                _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(
                    item.SkillOnUse.GetArea(this, CurrentPosition, CurrentDirection, map), item.SkillOnUse.Color));
                if (_entity.VisibleByPlayer.CurrentValue)
                    await UniTask.WhenAll(item.Use(this, CurrentPosition, direction, map),
                        UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
                else
                    await item.Use(this, CurrentPosition, direction, map);

                State = CharacterState.Wait;
            }
            else
            {
                throw new Exception("item cannot use");
            }
        }

        public async UniTask ThrowItem(int itemIndex, Direction8 direction, IMap world)
        {
            var item = _inventory.Remove(itemIndex);
            if (item == null) throw new Exception("item is null");
            Log.Debug($"[Action]{_name}:ThrowItem\n{item.Info()}\n direction:{direction}");
            Turn(direction);
            var itemEntity = world.SpawnItem(item, CurrentPosition);
            GameLog.Add($"{_name}:{item.Name}を投げた");
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(itemEntity.Throw(this, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await itemEntity.Throw(this, direction, world);

            State = CharacterState.Wait;
        }

        public UniTask<int> GainHp(int value)
        {
            return _statusManager.GainHp(value);
        }

        public void Dispose()
        {
            _entity.Dispose();
            _inventory.Dispose();
            _onEffectSpawned.Dispose();
            _direction.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Entity Entity => _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public ISkill[] Skills => _skills;

        public IVisionRange Area => _area;

        public CharacterMemento Serialize()
        {
            return new CharacterMemento(
                _name,
                CharacterType,
                Behavior.WanderAround,
                _statusManager.Serialize(),
                _entity.Serialize(),
                _skills.Select(x => x.Serialize()).ToArray(),
                _inventory.Serialize(),
                _affiliationManager.Serialize(),
                Aggression,
                IsLeader,
                IsBoss
            );
        }

        public async UniTask BlowAway(Direction8 direction, int distance, IPassableChecker map)
        {
            for (var i = 0; i < distance; i++)
            {
                if (!CanMove(direction, map))
                    break;
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);

            State = CharacterState.Wait;
        }

        public int CurrentMaxHp => _statusManager.CurrentMaxHp;
        public int CurrentHp => _statusManager.CurrentHp;

        public UniTask<int> LoseHp(int value)
        {
            return _statusManager.LoseHp(value);
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _statusManager.AddCondition(condition, removalCondition);
        }

        public async UniTask ForceMove(Direction8 direction, IInput input)
        {
            Turn(direction);
            await _entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);
        }

        public void WasAttackedBy(IActorOfEffect actor, float impact)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(actor.CurrentPosition - CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }

            _affiliationManager.OnCharacterAttacked(actor.Affiliation, Affiliation, impact);
        }

        public void WasHealedBy(IActorOfEffect actor, float impact)
        {
            _affiliationManager.OnCharacterHealed(actor.Affiliation, Affiliation, impact);
        }

        ~Character()
        {
            Dispose();
        }

        public async UniTask DoNextAction(IMap world, IInput input)
        {
            State = CharacterState.Think;
            var action = await Behavior.GenerateNextAction(this, world, input);
            if (_isConfused)
            {
                action = RegenerateConfuseAction(this, world, action);
            }

            State = CharacterState.Act;
            await action.Do(this, world, input);
        }

        private IAction RegenerateConfuseAction(IHasBehavior character, IMap world, IAction action)
        {
            switch (action)
            {
                case Move _:
                case Swap _:
                    var moves = new List<IAction>();
                    foreach (var direction in DirectionMethods.AllDirections)
                    {
                        var move = new Move(direction);
                        var swap = new Swap(direction);
                        if (move.Doable(character, world))
                            moves.Add(move);
                        else if (swap.Doable(character, world))
                            moves.Add(swap);
                    }

                    return moves.GetAtRandom();

                case UseSkill useSkill:
                    return useSkill with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case UseItem useItem:
                    return useItem with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case ThrowItem throwItem:
                    return throwItem with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case DoNothing _:
                    return action;

                default:
                    throw new InvalidOperationException();
            }
        }

        public bool TryPickUp(IItem item)
        {
            if (_inventory.TryAdd(item))
            {
                _onPickUpItem.OnNext(Unit.Default);
                return true;
            }

            return false;
        }

        public IItem? ReplaceInventory(IItem? item, int index)
        {
            return _inventory.Replace(item, index);
        }

        public void RepairAllItem()
        {
            _inventory.RepairAll();
        }

        public void UpdateTurn(IMap world)
        {
            _statusManager.UpdateTurn();
            _affiliationManager.UpdateTurn(world.GetVisibleCharacters(this).Select(x => x.Affiliation));
        }

        public void AddMoney(int value)
        {
            _money += value;
        }

        public void ReduceMoney(int value)
        {
            _money -= value;
        }
    }
}