#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
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
using Utilities.Serialize.Option;

namespace Domain.Service.Characters
{
    internal sealed class Character : ICharacter
    {
        private readonly string _name;
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly Aggression _aggression;
        private readonly ReactiveProperty<Direction8> _direction;
        public EntityBase Entity { get; init; }
        private readonly Inventory _inventory;
        private readonly ObservableHashSet<string> _knownItemNames = new();
        private readonly Subject<Unit> _onAttacked = new();
        private readonly List<CharacterSkill> _skills;
        private readonly SpawnEffectSkill? _lastSkill;
        private readonly CharacterStatusManager _statusManager;
        public int DropExp { get; init; }
        private readonly ObservableList<IPlayerEvent> _events = new();
        private IMap _map;
        private readonly Subject<Unit> _onDead = new();
        private Option<UseSkill> _chargeAction = Option.None<UseSkill>();
        private ReactiveProperty<int> _chargeTurn = new(0);

        internal Character(CharacterMemento data, ICharacterBehavior behavior, IGameManager gameManager, IMap map, bool isPlayer)
        {
            IsPlayer = isPlayer;
            _name = data.Name;
            CharacterType = data.CharacterType;
            Entity = new EntityBase(data.Entity);
            _direction = new ReactiveProperty<Direction8>(data.Direction);
            _statusManager = new CharacterStatusManager(data.Status, Entity.Position, this, map);
            _skills = data.Skills.Select(x => new CharacterSkill(x)).ToList();
            _lastSkill = data.LastSkill.HasValue ? new SpawnEffectSkill(data.LastSkill.Value) : null;
            _inventory = new Inventory(data.Inventory, this);
            _knownItemNames = new ObservableHashSet<string>(data.KnownItemNames);
            _behavior = behavior;
            _canThroughWalls = data.CanThroughWalls;
            _affiliationManager = new CharacterAffiliationManager(Entity.Id, data.Affiliation, map.Player);
            _aggression = data.Aggression;
            DropExp = data.DropExp;
            IsLeader = data.IsLeader;
            IsShiny = data.IsShiny;
            IsBoss = data.IsBoss;
            IsFlying = data.IsFlying;
            CanPickUp = data.CanPickUp;
            CanUseItem = data.CanUseItem;

            _map = map;

            HasEvent = _events.ObserveCountChanged().Select(x => x > 0).ToReadOnlyReactiveProperty();

            Observable.Merge(
                AutoIdentify.Where(autoIdentify => autoIdentify).AsUnitObservable(),
                Settings.WorldSettings.AutoIdentify.Value.Where(autoIdentify => autoIdentify).AsUnitObservable()
            ).Subscribe(_ =>
            {
                foreach (var item in Inventory.AllItems)
                {
                    KnowItem(item, false);
                }
            });
        }

        public Location CurrentLocation => new(_map.Id, Entity.CurrentPosition);
        public bool IsDead => _statusManager.IsDead || Entity.IsDestroyed;
        private ICharacterBehavior _behavior { get; }
        public bool IsPlayer { get; init; }
        public bool IsLeader { get; init; }
        public bool IsShiny { get; init; }
        public bool IsBoss { get; init; }
        public bool IsFlying { get; init; }
        public bool _canThroughWalls { get; init; }
        public bool CanThroughWalls => _canThroughWalls ? true : IsPlayer && Settings.WorldSettings.IgnoreWall.CurrentValue;
        public bool CanPickUp { get; init; }
        public bool CanUseItem { get; init; }
        public ReadOnlyReactiveProperty<bool> AutoIdentify => _statusManager.GetFlagProperty(FlagStatType.AutoIdentify);
        public CharacterState State { get; set; } = CharacterState.Wait;
        public IReadOnlyList<IPlayerEvent> Events => _events;
        public ReadOnlyReactiveProperty<bool> HasEvent { get; init; }

        public void SetWaitState()
        {
            State = CharacterState.Wait;
        }

        public string GetName(IPlayer player)
        {
            return GetName(player, false);
        }

        public string GetNameIgnoreVisibility(IPlayer player)
        {
            return GetName(player, true);
        }

        public string GetName(IPlayer player, bool ignoreVisibility)
        {
            if (!ignoreVisibility && !Entity.IsVisible)
            {
                return "何者か";
            }

            return Affiliation.GetAffiliationType(player.Character.Affiliation) switch
            {
                AffiliationType.Ally => _name.SetColored(Colors.Green),
                AffiliationType.Enemy => _name.SetColored(Colors.Red),
                _ => _name.SetColored(Colors.SkyBlue)
            };
        }

        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<Unit> OnAttacked => _onAttacked;
        public Observable<OnStartItemSelectMessage> OnStartItemSelect => _behavior.OnStartItemSelect;
        public Observable<Unit> OnSelectedItemSelect => _behavior.OnSelectedItemSelect;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        public Observable<OnChargeActionUpdatedMessage> OnChargeActionUpdated =>
            _chargeTurn.Select(x => new OnChargeActionUpdatedMessage(
                x,
                _chargeAction.Map(
                    skill => new ChargedActionPreviewEffectData(
                        skill.Skill.GetArea(this, Entity.CurrentPosition, skill.Direction, _map, true),
                        skill.Skill.Color
                    )
                ).Value
            ));
        public ICharacterType CharacterType { get; init; }
        public IStatusManager Status => _statusManager;
        public Aggression Aggression => _aggression;
        public IAffiliation Affiliation => _affiliationManager;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;
        public Observable<Unit> OnDead => _onDead;
        public IReadOnlyList<ICharacterSkill> Skills => _skills;
        public IVisionRange VisionRange => _statusManager.VisionRange;
        public IEnumerable<Vector2Int> VisibleArea => _statusManager.VisionRange.VisibleArea;

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

        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls,
            IPassableChecker map)
        {
            if (canThroughWalls)
            {
                return map.At(position + direction.Vector())
                    .CanPlace(isFlying, canThroughWalls, false, EntityLayer.Middle);
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
            if (target.IsPlayer)
                return false;
            return target.CanMoveIgnoreEntity(destination, direction.Reverse(), map) &&
                   CanMoveIgnoreEntity(position, direction, map);
        }

        public bool CanMoveIgnoreEntity(Direction8 direction, IPassableChecker map)
        {
            return CanMoveIgnoreEntity(Entity.CurrentPosition, direction, map);
        }

        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            if (CanThroughWalls)
                return map.At(position + direction.Vector())
                    .CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle);

            return map.At(position + direction.Vector()).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle)
                   && (!direction.IsDiagonal() ||
                       (map.At(position + direction.Rotate45Clockwise().Vector()).IsPassableOnMap() &&
                        map.At(position + direction.Rotate45AntiClockwise().Vector()).IsPassableOnMap()));
        }

        #endregion

        #region Action

        public void CancelChargeAction()
        {
            _chargeAction = Option.None<UseSkill>();
            _chargeTurn.Value = 0;
        }

        public async UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input)
        {
            State = CharacterState.Think;
            if (_chargeTurn.Value > 0)
            {
                _chargeTurn.Value--;
            }

            if (_chargeAction.HasValue && _chargeTurn.Value == 0)
            {
                State = CharacterState.Act;
                await _chargeAction.Value.Do(this, map, input);
                _chargeAction = Option.None<UseSkill>();
            }
            else if (_chargeTurn.Value > 0)
            {
                DoNothing();
            }
            else
            {
                var action = await _behavior.GenerateNextAction(this, gameManager, map, input);
                if (Status.IsFlagStat(FlagStatType.Confused))
                {
                    action = RegenerateConfuseAction(map, action);
                }

                if (action is UseSkill useSkill && useSkill.Skill.ChargeTurn > 0)
                {
                    _chargeAction = Option.Some(useSkill);
                    _chargeTurn.Value = useSkill.Skill.ChargeTurn;
                    DoNothing();
                    return;
                }

                State = CharacterState.Act;
                await action.Do(this, map, input);
            }
        }

        private IAction RegenerateConfuseAction(IMap map, IAction action)
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
                        if (move.Doable(this, map))
                            moves.Add(move);
                        else if (swap.Doable(this, map))
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
                .OrderBy(x =>
                    VectorExtension.ChebyshevDistance(x.character.Entity.CurrentPosition, Entity.CurrentPosition))
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
            Log.Debug(
                $"[Action]{_name}:Move direction:{direction} destination:{Entity.CurrentPosition + direction.Vector()}");
            Turn(direction);
            await Entity.Move(direction,
                input.IsDash() ? Settings.GlobalSettings.DashMilliseconds.CurrentValue : Settings.GlobalSettings.MoveMilliseconds.CurrentValue);

            State = CharacterState.Finish;
        }

        public async UniTask ForceMove(Direction8 direction, IInput input)
        {
            State = CharacterState.Act;
            Turn(direction);
            await Entity.Move(direction,
                input.IsDash() ? Settings.GlobalSettings.DashMilliseconds.CurrentValue : Settings.GlobalSettings.MoveMilliseconds.CurrentValue);

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
                if (CanMove(direction, map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                    await Entity.Move(direction, Settings.GlobalSettings.ThrowMilliseconds.CurrentValue, true);
            }

            if (IsDead)
            {
                State = CharacterState.Finish;
                return;
            }

            var result = await skill.Use(this, Entity.CurrentPosition, direction, map);
            if (result.Result == SkillResult.Success)
            {
                _onAttacked.OnNext(Unit.Default);
            }

            for (var i = 0; i < skill.BackStepDistance; i++)
            {
                if (CanMove(direction.Reverse(), map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                    await Entity.Move(direction.Reverse(), Settings.GlobalSettings.ThrowMilliseconds.CurrentValue, true);
            }

            State = CharacterState.Finish;
        }

        public async UniTask UseLastSkill()
        {
            if (_lastSkill != null)
            {
                await _lastSkill.Use(this, Entity.CurrentPosition, CurrentDirection, _map);
            }
        }

        public async UniTask UseItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseItem\n{item.Info(map.Player, map.ItemPlaceholders)}\ndirection:{direction}");
            Turn(direction);

            GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を使った");
            if (item.CanActivateWhenUsed)
            {
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
                    async itemTarget => await item.Use(this, Entity.CurrentPosition, direction, map),
                    async inventoryTarget => await item.Use(this, Entity.CurrentPosition, direction, map)
                );
                if (result.Result == SkillResult.Success)
                {
                    if (!IsKnownItem(item) && item.IdentifyIfUsed)
                    {
                        KnowItem(item, true);
                    }
                }
            }

            State = CharacterState.Finish;
        }

        public async UniTask UseItemOnDeath()
        {
            foreach (var item in Inventory.AllItems.Where(x => x.UseOnDeath))
            {
                await UseItem(item, CurrentDirection, _map);
                if (!IsDead)
                    break;
            }
        }

        public async UniTask ThrowItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug(
                $"[Action]{_name}:ThrowItem\n{item.Info(map.Player, map.ItemPlaceholders)}\n direction:{direction}");
            Turn(direction);
            if (item.CannotDropIfCursed)
            {
                item.SetCurseIdentified(true);
                if (item.IsCursed)
                {
                    GameLog.Add(Entity.IsVisible, $"{item.GetName(map.Player, map.ItemPlaceholders)}は呪われていて投げられない");
                    State = CharacterState.Finish;
                    return;
                }
            }

            GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を投げた");

            if (_inventory.Contains(item))
            {
                _inventory.Remove(item);
            }
            else
            {
                map.TryPickUpAt(Entity.CurrentPosition, true);
            }

            var destination =
                ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, CommonSenseParameters.ThrowDistance,
                    map);

            _onAttacked.OnNext(Unit.Default);

            if (Entity.IsVisible && destination != Entity.CurrentPosition)
            {
                await map.ShowThrowAnimation(item.Icon, Entity.CurrentPosition, direction,
                    CommonSenseParameters.ThrowDistance, EntityLayer.Middle);
            }

            var itemEntity = map.SpawnItem(item,
                map.FindBlankPositionFrom(destination, position => map.At(position).IsBlank(EntityLayer.Bottom)));

            await map.ExecuteTrapAt(destination, this);
            item = itemEntity.Item;
            if (item.CanActivateWhenThrown)
            {
                var result = await item.UseWhenThrown(this, destination, direction, map);
            }

            State = CharacterState.Finish;
        }

        public void ForceDropItem(int index, IMap map)
        {
            if (Inventory.CanRemove(index))
            {
                var item = Inventory.Remove(index);
                GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を落とした");
                map.SpawnItem(item,
                    map.FindBlankPositionFrom(Entity.CurrentPosition,
                        position => map.At(position).IsBlank(EntityLayer.Bottom)));
                State = CharacterState.Finish;
            }
        }
        public void PickUpItem(IMap map)
        {
            var groundItem = map.Items.At(Entity.CurrentPosition).First();

            if (Inventory.CanAddToEmpty())
            {
                map.TryPickUpAt(Entity.CurrentPosition, true);
                Inventory.AddToEmpty(groundItem.Item);
                GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{groundItem.Item.GetName(map.Player, map.ItemPlaceholders)}を拾った");
            }
            else
            {
                throw new Exception("Can't add item to inventory");
            }

            State = CharacterState.Finish;
        }
        public void DropItem(IItem item, IMap map)
        {
            if (item.CannotDropIfCursed)
            {
                item.SetCurseIdentified(true);
                if (item.IsCursed)
                {
                    GameLog.Add(Entity.IsVisible, $"{item.GetName(map.Player, map.ItemPlaceholders)}は呪われていて捨てられない");
                    State = CharacterState.Finish;
                    return;
                }
            }

            var groundItem = map.Items.At(Entity.CurrentPosition).FirstOrDefault();
            var index = Inventory.GetItemIndex(item).Value;

            if (Inventory.CanReplaceOrRemove(groundItem?.Item, index))
            {
                var replacedItem = Inventory.ReplaceOrRemove(groundItem?.Item, index);
                if (groundItem != null)
                {
                    map.TryPickUpAt(Entity.CurrentPosition, true);
                    GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{groundItem.Item.GetName(map.Player, map.ItemPlaceholders)}を拾った");
                }
                GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は{replacedItem.GetName(map.Player, map.ItemPlaceholders)}を捨てた");
                map.SpawnItem(replacedItem,
                    map.FindBlankPositionFrom(Entity.CurrentPosition,
                        position => map.At(position).IsBlank(EntityLayer.Bottom)));
            }
            else
            {
                throw new Exception("Can't replace or remove item in inventory");
            }

            State = CharacterState.Finish;
        }

        public float EvaluateThrow(IItem item, Direction8 direction, IMap map)
        {
            return ItemEntity.EvaluateThrow(item, Entity.CurrentPosition, this, direction,
                CommonSenseParameters.ThrowDistance, map);
        }

        #endregion

        public void Dispose()
        {
            Entity.Dispose();
            _inventory.Dispose();
            _direction.Dispose();
        }

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
                DropExp,
                IsLeader,
                IsShiny,
                IsBoss,
                IsFlying,
                _canThroughWalls,
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
                await Entity.Move(direction, Settings.GlobalSettings.ThrowMilliseconds.CurrentValue, true);
            }

            if (!map.At(Entity.CurrentPosition).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle))
            {
                var position = map.FindBlankPositionFrom(Entity.CurrentPosition,
                    position => map.At(position).IsBlank(EntityLayer.Middle));
                Entity.Teleport(position);
            }
        }

        public void Die(string causeOfDamageLog)
        {
            _onDead.OnNext(Unit.Default);
            Entity.Destroy(causeOfDamageLog);
        }

        #region Status

        public int CurrentMaxHp => _statusManager.Hp.Max.CurrentIntValue;
        public int CurrentHp => _statusManager.Hp.Value.CurrentValue;

        public int GainHp(int value)
        {
            return _statusManager.GainHp(value);
        }

        public async UniTask<int> LoseHp(int value, string causeOfDamageLog)
        {
            return await _statusManager.LoseHp(value, causeOfDamageLog);
        }

        public void RestoreToFullHealth()
        {
            _statusManager.RestoreToFullHealth();
        }

        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition)
        {
            _statusManager.AddCondition(actor, condition);
        }

        public void ClearCondition()
        {
            _statusManager.ClearCondition();
        }

        #endregion

        #region ItemKnowledge

        public void KnowItem(IItem item, bool log)
        {
            if (IsPlayer)
            {
                if (!IsKnownItem(item) && log)
                {
                    GameLog.Add(Entity.IsVisible, $"{item.UnknownName(_map.ItemPlaceholders)}は{item.RevealedName}だった");
                }
                _knownItemNames.Add(item.BaseName);
            }
        }

        public bool IsKnownItem(IItem item)
        {
            return _knownItemNames.Contains(item.BaseName) || Settings.WorldSettings.AutoIdentify.CurrentValue;
        }

        public void ClearKnownItems(IMap map)
        {
            _knownItemNames.Clear();
            map.ItemPlaceholders.ClearPlayerAssignedNames();
            GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}はアイテムの名前を忘れてしまった");
        }

        #endregion

        public void ListenToAlert(Location location)
        {
            _statusManager.RemoveConditionType(typeof(Slept));
            _behavior.KnowLocationOf(location);
        }

        public void OnAttackedBy(IActorOfEffect actor, float impact)
        {
            var direction =
                DirectionMethods.NearestDirectionFromVector(actor.Entity.CurrentPosition - Entity.CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }

            _affiliationManager.OnCharacterAttacked(actor.Affiliation, Affiliation, impact);
            _statusManager.WasAttacked();
        }

        public void OnHealedBy(IActorOfEffect actor, float impact)
        {
            _affiliationManager.OnCharacterHealed(actor.Affiliation, Affiliation, impact);
        }

        public void OnEnemyDefeated(ITargetOfEffect target)
        {
            GainExp(target.DropExp);
        }

        public void GainExp(int value)
        {
            var level = _statusManager.Level.CurrentValue;
            GameLog.Add(Entity.IsVisible, $"{GetName(_map.Player)}は{value}の経験値を得た");
            _statusManager.GainExp(value);
            if (level < _statusManager.Level.CurrentValue)
            {
                GameLog.Add(Entity.IsVisible, $"{GetName(_map.Player)}はレベルアップした");
                _statusManager.LevelUp(_statusManager.Level.CurrentValue - level);
            }
        }

        public void ClearAffiliation(IMap map)
        {
            _affiliationManager.Clear();
            GameLog.Add(Entity.IsVisible, $"{GetName(map.Player)}は他のキャラクターのことを忘れてしまった");
        }

        public bool CanPickUpItem()
        {
            return _inventory.HasEmptySpace();
        }

        public bool TryPickUpItem(IMap map, bool canPickUpShopItem)
        {
            if (!CanPickUpItem())
                return false;
            var item = map.TryPickUpAt(Entity.CurrentPosition, canPickUpShopItem);
            if (item == null)
            {
                return false;
            }
            if (!Inventory.CanAddToEmpty())
                return false;
            Inventory.AddToEmpty(item.Item);
            return true;
        }

        public void AddEvent(IPlayerEvent ev)
        {
            _events.Add(ev);
        }

        public async UniTask<ItemFocus> SelectItem(string text, params ItemFocus[] disabledItems)
        {
            return await _behavior.SelectItem(text, disabledItems);
        }

        public async UniTask<ItemFocus> SelectItemWithCanSelect(string text, IPlayer player, IMap map, Func<IItem, bool> canSelect)
        {
            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var (item, index) in Inventory.AllItemsWithIndex)
            {
                if (!canSelect(item))
                {
                    disabledItemIndexes.Add(new ItemFocus(index));
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem == null || !canSelect(groundItem))
            {
                disabledItemIndexes.Add(ItemFocus.GroundItem);
            }

            return await SelectItem(text, disabledItemIndexes.ToArray());
        }

        public async UniTask UpdateTurn()
        {
            var visibleCharacters = _map.GetVisibleCharacters(this);
            await _statusManager.UpdateTurn(this, visibleCharacters.Any());
            _affiliationManager.UpdateTurn(visibleCharacters.Select(x => x.Affiliation));
            _inventory.UpdateTurn();
            if (_statusManager.IsFlagStat(FlagStatType.RandomTeleport) && RandUtils.IsLessThanProbability(CommonSenseParameters.RandomTeleportProbability))
            {
                var skill = new CharacterSkill(
                    CharacterSkill.Build(
                        new SpawnEffectSkillMemento(
                            new AtFeet(),
                            new SelfArea(),
                            new List<IEffect> { new TeleportEffect() },
                            1,
                            1,
                            "はテレポートした"
                        ),
                        0,
                        0,
                        0,
                        0
                    )
                );
                await UseSkill(skill, CurrentDirection, _map);
            }
            if (_statusManager.IsFlagStat(FlagStatType.RandomExplosion) && RandUtils.IsLessThanProbability(CommonSenseParameters.RandomExplosionProbability))
            {
                var skill = new CharacterSkill(
                    CharacterSkill.Build(
                        new SpawnEffectSkillMemento(
                            new AtFeet(),
                            new CircleArea(2, true, false),
                            new List<IEffect>
                            {
                                new PercentageDamageEffect(0.25f),
                                new BreakEffect(false, true, true, true, true, true)
                            },
                            1,
                            1,
                            "は爆発した"
                        ),
                        0,
                        0,
                        0,
                        0
                    )
                );
                await UseSkill(skill, CurrentDirection, _map);
            }
        }

        public void UpdateCharacterTurn()
        {
            _skills.ForEach(x => x.CoolDown());
        }

        ~Character()
        {
            Dispose();
        }

        public string Info()
        {
            var info = $"{_name}\n";
            return info;
        }
    }
}