#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Setting;
using Domain.Service.Action;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Entities;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Domain.Model.Action;
using Domain.Model.Message;
using Domain.Model.Item;

namespace Domain.Service.Characters
{
    internal sealed class Character : ICharacter
    {
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly Aggression _aggression;
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Entity _entity;
        private readonly Inventory _inventory;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        private readonly Subject<Unit> _onPickUpItem = new();
        private readonly ISkill[] _skills;
        private readonly ISkill? _lastSkill;
        private readonly CharacterStatusManager _statusManager;
        private bool _canIgnoreWall;
        private int _money;
        private string _name = "Character";
        private readonly IDisposable _disposable;
        private IMap _map;

        internal Character(CharacterMemento data, ICharacterBehavior behavior, Observable<bool> canIgnoreWall,
            IMap map)
        {
            _name = data.Name;
            CharacterType = data.CharacterType;
            _entity = new Entity(data.Entity);
            _statusManager = new CharacterStatusManager(data.Status, Position, map);
            _skills = data.Skills.Select(x => new Skill(x)).ToArray();
            _lastSkill = data.LastSkill != null ? new Skill(data.LastSkill) : null;
            _inventory = new Inventory(data.Inventory, _statusManager);
            Behavior = behavior;
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
            _affiliationManager = new CharacterAffiliationManager(Id, data.Affiliation, map.Player?.Affiliation);
            _aggression = data.Aggression;
            _money = data.Money;
            IsLeader = data.IsLeader;
            IsShiny = data.IsShiny;
            IsBoss = data.IsBoss;
            CanPickUp = data.CanPickUp;
            CanUseItem = data.CanUseItem;

            _disposable = OnDead.Subscribe(_ => Entity.Destroy());
            _map = map;
        }

        private bool _canAct => _statusManager.Conditions.All(condition => condition.CanAct);
        private bool _isConfused => _statusManager.Conditions.Any(condition => condition.CausesConfusion);
        private ICharacterBehavior Behavior { get; }
        public Entity Entity => _entity;
        public bool IsLeader { get; init; }
        public bool IsShiny { get; init; }
        public bool IsBoss { get; init; }
        public bool CanPickUp { get; init; }
        public bool CanUseItem { get; init; }
        public CharacterState State { get; set; } = CharacterState.Wait;
        public int Money => _money;

        public string GetName(IHasAffiliation player)
        {
            if (Affiliation.IsAlly(player.Affiliation))
                return _name.SetColored(Colors.Green);
            else if (Affiliation.IsEnemy(player.Affiliation))
                return _name.SetColored(Colors.Red);
            return _name.SetColored(Colors.SkyBlue);
        }

        public bool CanAct => _canAct;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;
        public Observable<Unit> OnDead => _statusManager.OnDead;
        public Observable<Unit> OnPickUpItem => _onPickUpItem;
        public ICharacterType CharacterType { get; init; }
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
        public bool CanMove(Direction8 direction, IPassableChecker map) => CanMove(CurrentPosition, direction, map);
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            return _canIgnoreWall
                ? true
                : map.IsPassable(position + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (map.IsPassable(position + direction.Rotate45Clockwise().Vector()) &&
                       map.IsPassable(position + direction.Rotate45AntiClockwise().Vector())));
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
            Log.Debug($"[Action]{_name}:Move direction:{direction} destination:{CurrentPosition + direction.Vector()}");
            Turn(direction);
            await _entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);

            State = CharacterState.Wait;
        }

        public async UniTask UseSkill(ISkill skill, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseSkill\n{skill.Info()}\ndirection:{direction}");
            Turn(direction);
            for (var i = 0; i < skill.RushDistance; i++)
            {
                if (CanMove(direction, map))
                    await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }
            _onEffectSpawned.OnNext(
                new OnEffectSpawnedMessage(skill.GetArea(this, CurrentPosition, CurrentDirection, map), skill.Color));
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction, map),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await skill.Use(this, CurrentPosition, direction, map);

            State = CharacterState.Wait;
        }

        public async UniTask UseItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseItem\n{item.Info()}\ndirection:{direction}");
            Turn(direction);

            if (item.SkillOnUse != null)
            {
                GameLog.Add($"{GetName(map.Player)}は{item.Name}を使った");
                _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(
                    item.SkillOnUse.GetArea(this, CurrentPosition, CurrentDirection, map), item.SkillOnUse.Color));
                if (_entity.VisibleByPlayer.CurrentValue)
                    await UniTask.WhenAll(item.Use(this, CurrentPosition, direction, map, false),
                        UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
                else
                    await item.Use(this, CurrentPosition, direction, map, false);

                State = CharacterState.Wait;
            }
            else
            {
                throw new Exception("item cannot use");
            }
        }

        public async UniTask ThrowItem(IItem item, Direction8 direction, IMap world)
        {
            _inventory.Remove(item);
            Log.Debug($"[Action]{_name}:ThrowItem\n{item.Info()}\n direction:{direction}");
            Turn(direction);
            var itemEntity = world.SpawnItem(item, CurrentPosition);
            GameLog.Add($"<color=blue>{_name}</color>は{item.Name}を投げた");
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(itemEntity.Throw(this, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await itemEntity.Throw(this, direction, world);

            State = CharacterState.Wait;
        }

        public float EvaluateThrow(IItem item, Direction8 direction, IMap world)
        {
            return ItemEntity.EvaluateThrow(item, CurrentPosition, this, direction, world);
        }

        public UniTask<int> GainHp(int value)
        {
            return UniTask.FromResult(_statusManager.GainHp(value));
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _entity.Dispose();
            _inventory.Dispose();
            _onEffectSpawned.Dispose();
            _direction.Dispose();
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public ISkill[] Skills => _skills;

        public IVisionRange VisionRange => _statusManager.VisionRange;

        public CharacterMemento Serialize()
        {
            return new CharacterMemento(
                _name,
                CharacterType,
                Behavior.WanderAround,
                _statusManager.Serialize(),
                _entity.Serialize(),
                _skills.Select(x => x.Serialize()).ToArray(),
                _lastSkill?.Serialize(),
                _inventory.Serialize(),
                _affiliationManager.Serialize(),
                Aggression,
                _money,
                IsLeader,
                IsShiny,
                IsBoss,
                CanPickUp,
                CanUseItem
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

        public int CurrentMaxHp => _statusManager.Stats.CurrentMaxHp;
        public int CurrentHp => _statusManager.Stats.CurrentHp;
        public float AttackMultiplier => _statusManager.Stats.CurrentAttackMultiplier;

        public async UniTask<int> LoseHp(int value)
        {
            var result = _statusManager.LoseHp(value);
            if (_statusManager.IsDead && _lastSkill != null)
                await _lastSkill.Use(this, CurrentPosition, CurrentDirection, _map);
            return result;
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

        public void UpdateTurn()
        {
            _statusManager.UpdateTurn(_map.GetVisibleCharacters(this).Any(x => x.IsEnemy(this)));
            _affiliationManager.UpdateTurn(_map.GetVisibleCharacters(this).Select(x => x.Affiliation));
            _inventory.UpdateTurn();
        }

        public void AddMoney(int value)
        {
            Log.Debug($"{_name}:AddMoney {_money}+={value}");
            _money += value;
        }

        public void ReduceMoney(int value)
        {
            Log.Debug($"{_name}:ReduceMoney {_money}-={value}");
            _money -= value;
        }

        ~Character()
        {
            Dispose();
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
    }
}