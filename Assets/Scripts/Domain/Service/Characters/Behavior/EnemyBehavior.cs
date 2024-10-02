#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Action;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        public Observable<OnItemSelectMessage> OnItemSelect { get; init; } = new Subject<OnItemSelectMessage>();

        private ICharacter? _lastTarget;
        private Vector2Int? _lastTargetPosition;
        private readonly Option<Vector2Int> _homePosition;
        public Option<Vector2Int> HomePosition => _homePosition;

        private readonly float behavioralRandomness = 0.01f;

        private readonly IBehaviorWhenUndiscoveringTarget _wander;

        private readonly IBehaviorWhenDiscoveringTarget _discoveringLeader = new Chase();
        private readonly IBehaviorWhenDiscoveringTarget _returningHome = new Chase();
        private readonly IBehaviorWhenDiscoveringTarget _default;
        private readonly bool _prioritizeMovement = false;

        private readonly float _distanceTopBound = float.PositiveInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _greaterThanTopBound = null;
        private readonly bool _prioritizeMovementWhenDistanceGreaterThanTopBound = false;

        private readonly float _distanceBottomBound = float.NegativeInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _lessThanBottomBound = null;
        private readonly bool _prioritizeMovementWhenDistanceLessThanBottomBound = false;

        public BehaviorData BehaviorData { get; init; }

        public EnemyBehavior(BehaviorMemento data)
        {
            BehaviorData = data.Behavior;
            _homePosition = data.HomePosition;
            _lastTargetPosition = data.LastTargetPosition.Value;

            if (BehaviorData.wanderAround)
            {
                _wander = new Wander();
            }
            else
            {
                _wander = new NoMove();
            }
            _default = BehaviorData.Default.ToDiscoveredTargetBehavior();
            _prioritizeMovement = BehaviorData.PrioritizeMovement;
            if (BehaviorData.UseTopBound)
            {
                _distanceTopBound = BehaviorData.distanceTopBound;
                _greaterThanTopBound = BehaviorData.greaterThanTopBound.ToDiscoveredTargetBehavior();
                _prioritizeMovementWhenDistanceGreaterThanTopBound = BehaviorData.PrioritizeMovementWhenDistanceGreaterThanTopBound;
            }
            if (BehaviorData.UseBottomBound)
            {
                _distanceBottomBound = BehaviorData.distanceBottomBound;
                _lessThanBottomBound = BehaviorData.lessThanBottomBound.ToDiscoveredTargetBehavior();
                _prioritizeMovementWhenDistanceLessThanBottomBound = BehaviorData.PrioritizeMovementWhenDistanceLessThanBottomBound;
            }
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(
                behavior: BehaviorData,
                homePosition: _homePosition,
                lastTargetPosition: _lastTargetPosition
            );
        }
        
        public static BehaviorMemento Build(BehaviorData behavior, Option<Vector2Int> homePosition)
        {
            return new BehaviorMemento(
                behavior: behavior,
                homePosition: homePosition,
                lastTargetPosition: null
            );
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap world, IInput input)
        {
            _lastTarget = null;
            HashSet<Vector2Int> visibleArea = new(character.VisionRange.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);

            var visibleCharacters = world.GetVisibleCharacters(character);
            var visibleEnemies = visibleCharacters.Where(c => character.IsEnemy(c));
            var visibleLeaders = visibleCharacters.Where(c => character.IsAlly(c) && c.IsLeader);
            if (character.IsAlly(world.Player))
            {
                visibleLeaders = visibleLeaders.Append(world.Player);
            }

            var targetedEnemy = GetTargetedEnemy(character, visibleEnemies);
            var targetedLeader = GetTargetedLeader(character, visibleLeaders);
            var targetedPosition = GetTargetPosition(character, world);

            if (BehaviorData.PrioritizeEnemiesOverLeaders)
            {
                if (targetedEnemy != null)
                {
                    DiscoverEnemy(world, targetedEnemy);
                }
                else if (targetedLeader != null)
                {
                    DiscoverLeader(world, targetedLeader);
                }
                else if (_lastTargetPosition.HasValue)
                {
                    _lastTargetPosition = GetTargetPosition(character, world);
                }
            }
            else
            {
                if (targetedLeader != null)
                {
                    DiscoverLeader(world, targetedLeader);
                }
                else if (targetedEnemy != null)
                {
                    DiscoverEnemy(world, targetedEnemy);
                }
                else if (_lastTargetPosition.HasValue)
                {
                    _lastTargetPosition = GetTargetPosition(character, world);
                }
            }

            if (_lastTargetPosition != null) //目指す座標がある
            {
                Log.Debug($"[Think] Target position is {_lastTargetPosition}.");
            }
            else if (HomePosition.HasValue)
            {
                Log.Debug($"[Think] Home position is {HomePosition}.");
                _lastTargetPosition = HomePosition.Value;
            }
            else
            {
                Log.Debug($"[Think] Wandering around.");
                _lastTargetPosition = null;
            }

            if (_lastTargetPosition != null)
            {
                var relativeVector = _lastTargetPosition.Value - character.CurrentPosition;
                if (Vector2Extension.ChebyshevDistance(relativeVector) <= 1)
                {
                    var direction = DirectionMethods.NearestDirectionFromVector(relativeVector);
                    if (direction.HasValue)
                        character.Turn(direction.Value);
                }
            }

            var actions = new List<IAction>();
            if (PrioritizeMovement(character, _lastTargetPosition))
            {
                actions.AddRange(GenerateMoveActionsDoable(character, _lastTargetPosition, world));
                if (!actions.Any(action => action.Evaluate(character, world) > 0))
                {
                    actions.AddRange(GenerateUseSkillActionsDoable(character, world));
                    actions.AddRange(GenerateUseItemActionsDoable(character, world));
                    actions.AddRange(GenerateThrowItemActionsDoable(character, world));
                }
            }
            else
            {
                actions.AddRange(GenerateUseSkillActionsDoable(character, world));
                actions.AddRange(GenerateUseItemActionsDoable(character, world));
                actions.AddRange(GenerateThrowItemActionsDoable(character, world));
                if (!actions.Any(action => action.Evaluate(character, world) > 0))
                {
                    actions.AddRange(GenerateMoveActionsDoable(character, _lastTargetPosition, world));
                }
            }

            var validActions = actions.Where(action => action.Evaluate(character, world) > 0);
            foreach (var actionTemp in validActions)
            {
                Log.Debug($"[Think] {actionTemp.Info()} {actionTemp.Evaluate(character, world)}");
            }

            var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness),
                new DoNothing()));
            return action;
        }

        private void DiscoverLeader(IMap world, ICharacter targetedLeader)
        {
            Log.Debug($"[Think] Discover Leader {targetedLeader.GetName(world.Player)}.");
            _lastTarget = targetedLeader;
            _lastTargetPosition = targetedLeader.CurrentPosition;
        }

        private void DiscoverEnemy(IMap world, ICharacter targetedEnemy)
        {
            Log.Debug($"[Think] Discover Enemy {targetedEnemy.GetName(world.Player)}.");
            _lastTarget = targetedEnemy;
            _lastTargetPosition = targetedEnemy.CurrentPosition;
        }

        public ICharacter? GetTargetedEnemy(IHasBehavior character, IEnumerable<ICharacter> visibleEnemies)
        {
            if (visibleEnemies.Any())
                return visibleEnemies.MinBy(enemy => character.Affiliation.GetAffection(enemy.Affiliation));
            return null;
        }

        public ICharacter? GetTargetedLeader(IHasBehavior character, IEnumerable<ICharacter> visibleLeaders)
        {
            if (visibleLeaders.Any())
                return visibleLeaders.MaxBy(leader => character.Affiliation.GetAffection(leader.Affiliation));
            return null;
        }

        public Vector2Int? GetTargetPosition(IHasBehavior character, IMap world)
        {
            if (_lastTargetPosition == null)
                return null;
            return GetDiscoveredTargetBehavior(character, _lastTargetPosition.Value) is Chase
                && character.CurrentPosition != _lastTargetPosition
                && world.IsReachable(character.CurrentPosition, _lastTargetPosition.Value, character.Affiliation) ?
                _lastTargetPosition : null;
        }

        private float GetDistance(IHasBehavior character, Vector2Int targetPosition)
        {
            var distance = Vector2Extension.ChebyshevDistance(character.CurrentPosition, targetPosition);
            return distance;
        }

        public IBehaviorWhenDiscoveringTarget GetDiscoveredTargetBehavior(IHasBehavior character, Vector2Int targetPosition)
        {
            if (_lastTarget != null && character.IsAlly(_lastTarget) && _lastTarget.IsLeader)
                return _discoveringLeader;

            if (_lastTarget == null)
                return _returningHome;

            var distance = GetDistance(character, targetPosition);
            if (_greaterThanTopBound != null && distance > _distanceTopBound)
                return _greaterThanTopBound;
            if (_lessThanBottomBound != null && distance < _distanceBottomBound)
                return _lessThanBottomBound;
            return _default;
        }

        public bool PrioritizeMovement(IHasBehavior character, Vector2Int? targetPosition)
        {
            if (targetPosition == null)
                return _prioritizeMovement;
            var distance = GetDistance(character, targetPosition.Value);
            if (distance > _distanceTopBound)
                return _prioritizeMovementWhenDistanceGreaterThanTopBound;
            if (distance < _distanceBottomBound)
                return _prioritizeMovementWhenDistanceLessThanBottomBound;
            return _prioritizeMovement;
        }

        private IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int? targetPosition,
            IMap world)
        {
            if (targetPosition != null)
            {
                return GetDiscoveredTargetBehavior(character, targetPosition.Value)
                    .GenerateMoveActionsDoable(character, targetPosition.Value, world);
            }
            else
            {
                return _wander.GenerateMoveActionsDoable(character, world);
            }
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap world)
        {
            return character.Skills
                .SelectMany(
                    skill => DirectionMethods.AllDirections
                        .Select(direction => new UseSkill(skill, direction))
                )
                .Where(action => action.Doable(character, world));
        }

        private IEnumerable<UseItem> GenerateUseItemActionsDoable(IHasBehavior character, IMap world)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<UseItem>();
            }

            return character.Inventory.AllItems
                .SelectMany(
                    item => DirectionMethods.AllDirections
                        .Select(direction => new UseItem(item, direction))
                )
                .Where(action => action.Doable(character, world));
        }

        private IEnumerable<ThrowItem> GenerateThrowItemActionsDoable(IHasBehavior character, IMap world)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<ThrowItem>();
            }

            return character.Inventory.AllItems
                .SelectMany(
                    item => DirectionMethods.AllDirections
                        .Select(direction => new ThrowItem(item, direction))
                )
                .Where(action => action.Doable(character, world));
        }

        public UniTask<IItem?> SelectItem(IInventory inventory, params int[] disabledItemIds)
        {
            return UniTask.FromResult<IItem?>(null);
        }
    }
}