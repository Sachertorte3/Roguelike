#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Effect;
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
        private readonly bool _prioritizeMovement;

        private readonly float _distanceTopBound = float.PositiveInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _greaterThanTopBound;
        private readonly bool _prioritizeMovementWhenDistanceGreaterThanTopBound;

        private readonly float _distanceBottomBound = float.NegativeInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _lessThanBottomBound;
        private readonly bool _prioritizeMovementWhenDistanceLessThanBottomBound;

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
                _prioritizeMovementWhenDistanceGreaterThanTopBound =
                    BehaviorData.PrioritizeMovementWhenDistanceGreaterThanTopBound;
            }

            if (BehaviorData.UseBottomBound)
            {
                _distanceBottomBound = BehaviorData.distanceBottomBound;
                _lessThanBottomBound = BehaviorData.lessThanBottomBound.ToDiscoveredTargetBehavior();
                _prioritizeMovementWhenDistanceLessThanBottomBound =
                    BehaviorData.PrioritizeMovementWhenDistanceLessThanBottomBound;
            }
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(
                BehaviorData,
                _homePosition,
                _lastTargetPosition
            );
        }

        public static BehaviorMemento Build(BehaviorData behavior, Option<Vector2Int> homePosition)
        {
            return new BehaviorMemento(
                behavior,
                homePosition,
                null
            );
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            _lastTarget = null;
            HashSet<Vector2Int> visibleArea = new(character.VisionRange.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);

            var visibleCharacters = map.GetVisibleCharacters(character);
            var visibleEnemies = visibleCharacters.Where(c => character.IsEnemy(c));
            var visibleLeaders = visibleCharacters.Where(c => character.IsAlly(c) && c.IsLeader);
            if (character.IsAlly(map.Player))
            {
                visibleLeaders = visibleLeaders.Append(map.Player);
            }

            var targetedEnemy = GetTargetedEnemy(character, visibleEnemies);
            var targetedLeader = GetTargetedLeader(character, visibleLeaders);
            var targetedPosition = GetTargetPosition(character, map);

            if (BehaviorData.PrioritizeEnemiesOverLeaders)
            {
                if (targetedEnemy != null)
                {
                    DiscoverEnemy(map, targetedEnemy);
                }
                else if (targetedLeader != null)
                {
                    DiscoverLeader(map, targetedLeader);
                }
                else if (_lastTargetPosition.HasValue)
                {
                    _lastTargetPosition = GetTargetPosition(character, map);
                }
            }
            else
            {
                if (targetedLeader != null)
                {
                    DiscoverLeader(map, targetedLeader);
                }
                else if (targetedEnemy != null)
                {
                    DiscoverEnemy(map, targetedEnemy);
                }
                else if (_lastTargetPosition.HasValue)
                {
                    _lastTargetPosition = GetTargetPosition(character, map);
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
                Log.Debug("[Think] Wandering around.");
                _lastTargetPosition = null;
            }

            if (_lastTargetPosition != null)
            {
                var relativeVector = _lastTargetPosition.Value - character.CurrentPosition;
                if (VectorExtension.ChebyshevDistance(relativeVector) <= 1)
                {
                    var direction = DirectionMethods.NearestDirectionFromVector(relativeVector);
                    if (direction.HasValue)
                        character.Turn(direction.Value);
                }
            }

            var actions = new List<IAction>();
            if (PrioritizeMovement(character, _lastTargetPosition))
            {
                actions.AddRange(GenerateMoveActionsDoable(character, _lastTargetPosition, map));
                if (!actions.Any(action => action.Evaluate(character, map) > 0))
                {
                    actions.AddRange(GenerateUseSkillActionsDoable(character, map));
                    actions.AddRange(GenerateUseItemActionsDoable(character, map));
                    actions.AddRange(GenerateThrowItemActionsDoable(character, map));
                }
            }
            else
            {
                actions.AddRange(GenerateUseSkillActionsDoable(character, map));
                actions.AddRange(GenerateUseItemActionsDoable(character, map));
                actions.AddRange(GenerateThrowItemActionsDoable(character, map));
                if (!actions.Any(action => action.Evaluate(character, map) > 0))
                {
                    actions.AddRange(GenerateMoveActionsDoable(character, _lastTargetPosition, map));
                }
            }

            var validActions = actions.Where(action => action.Evaluate(character, map) > 0);
            foreach (var actionTemp in validActions)
            {
                Log.Debug($"[Think] {actionTemp.Info()} {actionTemp.Evaluate(character, map)}");
            }

            var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                action => action.Evaluate(character, map) + Random.Range(0, behavioralRandomness),
                new DoNothing()));
            return action;
        }

        private void DiscoverLeader(IMap map, ICharacter targetedLeader)
        {
            Log.Debug($"[Think] Discover Leader {targetedLeader.GetName(map.Player)}.");
            _lastTarget = targetedLeader;
            _lastTargetPosition = targetedLeader.CurrentPosition;
        }

        private void DiscoverEnemy(IMap map, ICharacter targetedEnemy)
        {
            Log.Debug($"[Think] Discover Enemy {targetedEnemy.GetName(map.Player)}.");
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

        public Vector2Int? GetTargetPosition(IHasBehavior character, IMap map)
        {
            if (_lastTargetPosition == null)
                return null;
            return GetDiscoveredTargetBehavior(character, _lastTargetPosition.Value) is Chase
                   && character.CurrentPosition != _lastTargetPosition
                   && map.IsReachable(character.CurrentPosition, _lastTargetPosition.Value, character)
                ? _lastTargetPosition
                : null;
        }

        private float GetDistance(IHasBehavior character, Vector2Int targetPosition)
        {
            var distance = VectorExtension.ChebyshevDistance(character.CurrentPosition, targetPosition);
            return distance;
        }

        public IBehaviorWhenDiscoveringTarget GetDiscoveredTargetBehavior(IHasBehavior character,
            Vector2Int targetPosition)
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
            IMap map)
        {
            if (targetPosition != null)
            {
                return GetDiscoveredTargetBehavior(character, targetPosition.Value)
                    .GenerateMoveActionsDoable(character, targetPosition.Value, map);
            }

            return _wander.GenerateMoveActionsDoable(character, map);
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap map)
        {
            return character.Skills
                .SelectMany(
                    skill => DirectionMethods.AllDirections
                        .Select(direction => new UseSkill(skill, direction))
                )
                .Where(action => action.Doable(character, map));
        }

        private IEnumerable<UseItem> GenerateUseItemActionsDoable(IHasBehavior character, IMap map)
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
                .Where(action => action.Doable(character, map));
        }

        private IEnumerable<ThrowItem> GenerateThrowItemActionsDoable(IHasBehavior character, IMap map)
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
                .Where(action => action.Doable(character, map));
        }

        public void KnowLocationOf(IHasBehavior self, IActorOfEffect target)
        {
            if (self.IsEnemy(target))
            {
                _lastTargetPosition = target.CurrentPosition;
            }
        }

        public UniTask<IItem?> SelectItem(IInventory inventory, params int[] disabledItemIds)
        {
            return UniTask.FromResult<IItem?>(null);
        }
    }
}