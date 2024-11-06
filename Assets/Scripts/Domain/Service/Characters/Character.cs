#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Action;
using Domain.Service.Characters.Behavior;
using Domain.Service.Characters.Conditions;
using Domain.Service.Effect;
using Domain.Service.Items;
using Domain.Service.Logs;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    internal sealed class Character : ICharacter
    {
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly Aggression _aggression;
        private readonly ReactiveProperty<Direction8> _direction;
        public Entity Entity { get; init; }
        private readonly Inventory _inventory;
        private readonly ObservableHashSet<string> _knownItemNames = new();
        private readonly Subject<Unit> _onAttacked = new();
        private readonly Subject<Unit> _onPickUpItem = new();
        private readonly List<CharacterSkill> _skills;
        private readonly SpawnEffectSkill? _lastSkill;
        private readonly CharacterStatusManager _statusManager;
        private int _money;
        private string _name = "Character";
        private readonly IDisposable _disposable;
        private IMap _map;
        private readonly Subject<Unit> _onDead = new();

        internal Character(CharacterMemento data, ICharacterBehavior behavior, IMap map)
        {
            _name = data.Name;
            CharacterType = data.CharacterType;
            Entity = new Entity(data.Entity);
            _direction = new ReactiveProperty<Direction8>(data.Direction);
            _statusManager = new CharacterStatusManager(data.Status, Entity.Position, this, map);
            _skills = data.Skills.Select(x => new CharacterSkill(x)).ToList();
            _lastSkill = data.LastSkill.HasValue ? new SpawnEffectSkill(data.LastSkill.Value) : null;
            _inventory = new Inventory(data.Inventory, this);
            _knownItemNames = new ObservableHashSet<string>(data.KnownItemNames);
            _behavior = behavior;
            CanThroughWalls = data.CanThroughWalls;
            _affiliationManager = new CharacterAffiliationManager(Entity.Id, data.Affiliation, map.Player?.Affiliation);
            _aggression = data.Aggression;
            _money = data.Money;
            IsLeader = data.IsLeader;
            IsShiny = data.IsShiny;
            IsBoss = data.IsBoss;
            IsFlying = data.IsFlying;
            CanPickUp = data.CanPickUp;
            CanUseItem = data.CanUseItem;

            _disposable = OnDead.Subscribe(_ => Entity.Destroy());
            _map = map;

            _statusManager.Stats.HpValue.Where(x => x <= 0).Subscribe(async _ =>
            {
                if (IsDead)
                {
                    foreach (var item in Inventory.AllItems.Where(x => x.UseOnDeath))
                    {
                        await UseItem(item, CurrentDirection, _map);
                        if (!IsDead)
                            break;
                    }
                }

                if (IsDead)
                {
                    if (_lastSkill != null)
                        await _lastSkill.Use(this, Entity.CurrentPosition, CurrentDirection, _map);
                    _onDead.OnNext(Unit.Default);
                }
            });
        }

        public bool IsDead => _statusManager.IsDead || Entity.IsDestroyed.CurrentValue;
        private ICharacterBehavior _behavior { get; }
        public bool IsLeader { get; init; }
        public bool IsShiny { get; init; }
        public bool IsBoss { get; init; }
        public bool IsFlying { get; init; }
        public bool CanThroughWalls { get; init; }
        public bool CanPickUp { get; init; }
        public bool CanUseItem { get; init; }
        public CharacterState State { get; set; } = CharacterState.Wait;

        public void SetWaitState()
        {
            State = CharacterState.Wait;
        }

        public int Money => _money;

        public string GetName(IHasAffiliation player) => GetName(player, false);
        public string GetName(IHasAffiliation player, bool ignoreVisibility)
        {
            if (!ignoreVisibility && !Entity.Visibility.CurrentValue)
            {
                return "何者か";
            }

            return Affiliation.GetAffiliationType(player.Affiliation) switch
            {
                AffiliationType.Ally => _name.SetColored(Colors.Green),
                AffiliationType.Enemy => _name.SetColored(Colors.Red),
                _ => _name.SetColored(Colors.SkyBlue)
            };
        }

        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<Unit> OnAttacked => _onAttacked;
        public Observable<Unit> OnPickUpItem => _onPickUpItem;
        public Observable<OnItemSelectMessage> OnItemSelect => _behavior.OnItemSelect;
        public Observable<Unit> OnKnownItemUpdated => _knownItemNames.ObserveCountChanged().Select(_ => Unit.Default);
        public ICharacterType CharacterType { get; init; }
        public IItemSelector ItemSelector => _behavior;
        public IStatusManager StatusManager => _statusManager;
        public Aggression Aggression => _aggression;
        public IAffiliation Affiliation => _affiliationManager;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;

        #region CanMove
        /// <summary>
        ///     Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even
        ///     if the destination is not passable.
        ///     If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            return CanMove(position, direction, IsFlying, CanThroughWalls, map);
        }

        public bool CanMove(Direction8 direction, bool isFlying, bool canThroughWalls, IPassableChecker map)
        {
            return CanMove(Entity.CurrentPosition, direction, isFlying, canThroughWalls, map);
        }

        public bool CanMove(Direction8 direction, IPassableChecker map)
        {
            return CanMove(Entity.CurrentPosition, direction, IsFlying, CanThroughWalls, map);
        }

        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls, IPassableChecker map)
        {
            if (canThroughWalls)
            {
                return map.At(position + direction.Vector()).CanPlace(isFlying, canThroughWalls, false, EntityLayer.Middle);
            }

            return map.At(position + direction.Vector()).CanPlace(isFlying, canThroughWalls, false, EntityLayer.Middle)
                   && (!direction.IsDiagonal() ||
                       (map.At(position + direction.Rotate45Clockwise().Vector()).IsPassableOnMap() &&
                        map.At(position + direction.Rotate45AntiClockwise().Vector()).IsPassableOnMap()));
        }

        public bool CanSwap(Direction8 direction, IMap map)
        {
            return CanSwap(Entity.CurrentPosition, direction, map);
        }

        public bool CanSwap(Vector2Int position, Direction8 direction, IMap map)
        {
            var destination = position + direction.Vector();
            var target = map.Characters.At(destination).FirstOrDefault();
            if (target == null)
                return false;
            if (target.IsEnemy(this))
                return false;
            if (target == map.Player)
                return false;
            return target.CanMoveIgnoreEntity(destination, direction.Reverse(), map) &&
                   CanMoveIgnoreEntity(position, direction, map);
        }

        public bool CanMoveIgnoreEntity(Direction8 direction, IPassableChecker map) =>
            CanMoveIgnoreEntity(Entity.CurrentPosition, direction, map);

        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            if (CanThroughWalls)
                return map.At(position + direction.Vector()).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle);

            return map.At(position + direction.Vector()).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle)
                   && (!direction.IsDiagonal() ||
                       (map.At(position + direction.Rotate45Clockwise().Vector()).IsPassableOnMap() &&
                        map.At(position + direction.Rotate45AntiClockwise().Vector()).IsPassableOnMap()));
        }
        #endregion
        #region Action
        public async UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input)
        {
            State = CharacterState.Think;
            var action = await _behavior.GenerateNextAction(this, gameManager, map, input);
            if (StatusManager.IsConfused)
            {
                action = RegenerateConfuseAction(this, map, action);
            }

            State = CharacterState.Act;
            await action.Do(this, map, input);
        }

        private IAction RegenerateConfuseAction(IHasBehavior character, IMap map, IAction action)
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
                        if (move.Doable(character, map))
                            moves.Add(move);
                        else if (swap.Doable(character, map))
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

        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }

        public void FaceNearestCharacter(IMap map)
        {
            var nearestCharacterDirection = map.GetVisibleCharacters(this)
                .Where(x => x != this)
                .Select(x => (character: x,
                    direction: DirectionMethods.FromVectorStrict(x.Entity.CurrentPosition - Entity.CurrentPosition)))
                .Where(x => x.direction.HasValue)
                .OrderBy(x => VectorExtension.ChebyshevDistance(x.character.Entity.CurrentPosition, Entity.CurrentPosition))
                .ThenByDescending(x => CurrentDirection.AngleTo(x.direction.Value).Value)
                .FirstOrDefault().direction;
            if (nearestCharacterDirection.HasValue)
            {
                Turn(nearestCharacterDirection.Value);
            }
        }

        public void DoNothing()
        {
            Log.Debug($"[Action]{_name}:DoNothing");
            State = CharacterState.Finish;
        }

        public async UniTask Move(Direction8 direction, IInput input)
        {
            Log.Debug($"[Action]{_name}:Move direction:{direction} destination:{Entity.CurrentPosition + direction.Vector()}");
            Turn(direction);
            await Entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);

            State = CharacterState.Finish;
        }

        public async UniTask ForceMove(Direction8 direction, IInput input)
        {
            State = CharacterState.Act;
            Turn(direction);
            await Entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);

            State = CharacterState.Finish;
        }

        public void Teleport(Vector2Int position)
        {
            Entity.Teleport(position);

            State = CharacterState.Finish;
        }

        public async UniTask UseSkill(ICharacterSkill skill, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseSkill\n{skill.Info()}\ndirection:{direction}");
            Turn(direction);
            for (var i = 0; i < skill.RushDistance; i++)
            {
                if (CanMove(direction, map))
                    await Entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
            }

            var result = await skill.Use(this, Entity.CurrentPosition, direction, map);
            if (result.Result == SkillResult.Success)
            {
                _onAttacked.OnNext(Unit.Default);
            }

            for (var i = 0; i < skill.BackStepDistance; i++)
            {
                if (CanMove(direction.Reverse(), map))
                    await Entity.Move(direction.Reverse(), Settings.ThrowMilliseconds.Value, true);
            }

            State = CharacterState.Finish;
        }

        public async UniTask UseItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseItem\n{item.Info(map.Player, map.ItemPlaceholders)}\ndirection:{direction}");
            Turn(direction);

            if (item.CanActivateWhenUsed)
            {
                GameLog.Add($"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を使った");
                var result = await item.SkillOnUse.Expect("skill on use is null").Match(
                    async spawnEffect =>
                    {
                        var result = await item.Use(this, Entity.CurrentPosition, direction, map);
                        if (result.Result == SkillResult.Success)
                        {
                            _onAttacked.OnNext(Unit.Default);
                        }
                        return result;
                    },
                    async itemTarget => await item.Use(this, Entity.CurrentPosition, direction, map)
                );
                if (result.Result == SkillResult.Success)
                {
                    if (!IsKnownItem(item) && item.IdentifyIfUsed)
                    {
                        AddKnownItem(item);
                    }
                }
                State = CharacterState.Finish;
            }
            else
            {
                throw new Exception("item cannot use");
            }
        }

        public async UniTask ThrowItem(IItem item, Direction8 direction, IMap map)
        {
            if (item.IsCursed && item.CannotDropIfCursed)
            {
                GameLog.Add($"{item.GetName(map.Player, map.ItemPlaceholders)}は呪われていて投げられない");
            }
            else
            {
                Log.Debug($"[Action]{_name}:ThrowItem\n{item.Info(map.Player, map.ItemPlaceholders)}\n direction:{direction}");
                Turn(direction);
                GameLog.Add($"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を投げた");
                var destination =
                    ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, CommonSenseParameters.ThrowDistance, map);
                if (_inventory.Remove(item))
                {
                    if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
                    {
                        _onAttacked.OnNext(Unit.Default);
                        await map.ShowThrowAnimation(item.Icon, Entity.CurrentPosition, direction, CommonSenseParameters.ThrowDistance, EntityLayer.Middle);
                    }

                    var itemEntity = map.SpawnItem(item,
                        map.FindBlankPositionFrom(destination, position => map.At(position).IsBlank(EntityLayer.Bottom)));

                    await map.ExecuteTrapAt(destination, this);
                    item = itemEntity.Item;
                    if (item.CanActivateWhenThrown)
                    {
                        await item.UseWhenThrown(this, destination, direction, map);
                    }
                }
                else
                {
                    var itemEntity = map.Items.At(Entity.CurrentPosition).FirstOrDefault();
                    if (itemEntity != null)
                    {
                        await itemEntity.BlowAway(this, direction, CommonSenseParameters.ThrowDistance, map);
                    }
                }
            }

            State = CharacterState.Finish;
        }

        public void DropItem(int itemIndex, IMap map, bool isForced)
        {
            var item = Inventory.GetItem(itemIndex);
            if (item != null && isForced)
            {
                ReplaceInventory(null, itemIndex);
                GameLog.Add($"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を落とした");
                map.SpawnItem(item,
                    map.FindBlankPositionFrom(Entity.CurrentPosition,
                        position => map.At(position).IsBlank(EntityLayer.Bottom)));
            }
            else if (item != null && item.IsCursed && item.CannotDropIfCursed)
            {
                GameLog.Add($"{item.GetName(map.Player, map.ItemPlaceholders)}は呪われていて捨てられない");
            }
            else
            {
                var pickedUpItem = map.TryPickUpAt(Entity.CurrentPosition, true);
                if (pickedUpItem != null)
                {
                    GameLog.Add($"{GetName(map.Player)}は{pickedUpItem.Item.GetName(map.Player, map.ItemPlaceholders)}を拾った");
                }
                ReplaceInventory(pickedUpItem?.Item, itemIndex);
                if (item != null)
                {
                    GameLog.Add($"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を捨てた");
                    map.SpawnItem(item,
                        map.FindBlankPositionFrom(Entity.CurrentPosition,
                            position => map.At(position).IsBlank(EntityLayer.Bottom)));
                }
            }

            State = CharacterState.Finish;
        }

        public float EvaluateThrow(IItem item, Direction8 direction, IMap map)
        {
            return ItemEntity.EvaluateThrow(item, Entity.CurrentPosition, this, direction, CommonSenseParameters.ThrowDistance, map);
        }
        #endregion
        public void Dispose()
        {
            _disposable.Dispose();
            Entity.Dispose();
            _inventory.Dispose();
            _direction.Dispose();
        }

        public Observable<Unit> OnDead => _onDead;

        public IReadOnlyList<ICharacterSkill> Skills => _skills;

        public IVisionRange VisionRange => _statusManager.VisionRange;
        public IEnumerable<Vector2Int> VisibleArea => _statusManager.VisionRange.VisibleArea;

        public CharacterMemento Serialize()
        {
            return new CharacterMemento
            (
                _name,
                CharacterType,
                _behavior.Serialize(),
                _statusManager.Serialize(),
                Entity.Serialize(),
                _direction.CurrentValue,
                _skills.Select(x => x.Serialize()).ToList(),
                _lastSkill.ToOption().Map(x => x.Serialize()),
                _inventory.Serialize(),
                _knownItemNames.ToList(),
                _affiliationManager.Serialize(),
                Aggression,
                _money,
                IsLeader,
                IsShiny,
                IsBoss,
                IsFlying,
                CanThroughWalls,
                CanPickUp,
                CanUseItem
            );
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            for (var i = 0; i < distance; i++)
            {
                if (!CanMove(direction, true, CanThroughWalls, map))
                    break;
                await Entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
            }

            if (!map.At(Entity.CurrentPosition).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle))
            {
                var position = map.FindBlankPositionFrom(Entity.CurrentPosition,
                    position => map.At(position).IsBlank(EntityLayer.Middle));
                Entity.Teleport(position);
            }
        }
        #region Status
        public int CurrentMaxHp => _statusManager.Stats.CurrentMaxHp;
        public int CurrentHp => _statusManager.Stats.CurrentHp;

        public int GainHp(int value)
        {
            return _statusManager.GainHp(value);
        }

        public int LoseHp(int value)
        {
            return _statusManager.LoseHp(value);
        }

        public float GetStatValue(StatType statType)
        {
            return _statusManager.GetStatValue(statType);
        }

        public float GetElementAttackMultiplier(Element element)
        {
            return _statusManager.Stats.GetElementAttackMultiplier(element);
        }

        public float GetElementDamageRateMultiplier(Element element)
        {
            return _statusManager.Stats.GetElementDamageRateMultiplier(element);
        }

        public float GetConditionResistance(ConditionTemplate condition)
        {
            return _statusManager.Stats.GetConditionResistance(condition);
        }

        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition)
        {
            _statusManager.AddCondition(actor, condition, removalCondition);
        }

        public void ClearCondition()
        {
            _statusManager.ClearCondition();
        }
        #endregion
        #region ItemKnowledge
        public void AddKnownItem(IItem item)
        {
            if (!IsKnownItem(item))
            {
                GameLog.Add($"{item.UnknownName(_map.ItemPlaceholders)}は{item.RevealedName}だった");
                _knownItemNames.Add(item.BaseName);
            }
        }

        public bool IsKnownItem(IItem item)
        {
            return _knownItemNames.Contains(item.BaseName);
        }

        public void ClearKnownItems(IMap map)
        {
            _knownItemNames.Clear();
            GameLog.Add($"{GetName(map.Player)}はアイテムの名前を忘れてしまった");
        }
        #endregion
        public void ListenToAlert(Vector2Int position)
        {
            _statusManager.RemoveConditionType(typeof(Slept));
            _behavior.KnowLocationOf(position);
        }

        public void WasAttackedBy(IActorOfEffect actor, float impact)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(actor.Entity.CurrentPosition - Entity.CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }

            _affiliationManager.OnCharacterAttacked(actor.Affiliation, Affiliation, impact);
            _statusManager.WasAttacked();
        }

        public void WasHealedBy(IActorOfEffect actor, float impact)
        {
            _affiliationManager.OnCharacterHealed(actor.Affiliation, Affiliation, impact);
        }

        public void ClearAffiliation(IMap map)
        {
            _affiliationManager.Clear();
            GameLog.Add($"{GetName(map.Player)}は他のキャラクターのことを忘れてしまった");
        }

        public bool CanPickUpItem()
        {
            return _inventory.HasEmptySpace();
        }

        public bool TryAddToInventory(IItem item)
        {
            if (_inventory.TryAdd(item))
            {
                _onPickUpItem.OnNext(Unit.Default);
                if (!IsKnownItem(item) && item.IdentifyIfGot)
                {
                    AddKnownItem(item);
                }
                return true;
            }

            return false;
        }

        public IItem? ReplaceInventory(IItem? item, int index)
        {
            return _inventory.Replace(item, index);
        }

        public void UpdateTurn()
        {
            _statusManager.UpdateTurn(this, _map.GetVisibleCharacters(this).Any());
            _affiliationManager.UpdateTurn(_map.GetVisibleCharacters(this).Select(x => x.Affiliation));
            _inventory.UpdateTurn();
            _skills.ForEach(x => x.UpdateTurn());
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
    }
}