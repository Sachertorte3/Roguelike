#nullable enable
using System;
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
using Random = UnityEngine.Random;

namespace Domain.Service.Characters.Behavior
{

    public sealed class EnemyBehavior : ICharacterBehavior
    {
        public Observable<OnItemSelectMessage> OnItemSelect { get; init; } = new Subject<OnItemSelectMessage>();

        private BehaviorResult _previousResult;
        private readonly (Location Location, Vector2Int Position)? _homePosition;

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

        public EnemyBehavior(BehaviorMemento data, Location mapLocation)
        {
            BehaviorData = data.Behavior;
            if (data.HomePosition.HasValue && data.HomePosition.Value.Item1 == mapLocation)
            {
                _homePosition = data.HomePosition;
            }
            _previousResult = new BehaviorResult(
                data.PreviousState.Value,
                data.PreviousTargetPosition.Value
            );

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
                _previousResult.State,
                _previousResult.TargetPosition
            );
        }

        public static BehaviorMemento Build(BehaviorData behavior, (Location, Vector2Int)? homePosition)
        {
            var memento = new BehaviorMemento(
                behavior,
                homePosition,
                null,
                null
            );
            var json = JsonUtility.ToJson(memento);
            return JsonUtility.FromJson<BehaviorMemento>(json);
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            var result = GenerateNextBehaviorResult(character, map);
            Log.Debug($"[Think] Result: {result.State} {result.TargetPosition}");

            if (result.TargetPosition != null)
            {
                var relativeVector = result.TargetPosition.Value - character.Entity.CurrentPosition;
                if (VectorExtension.ChebyshevDistance(relativeVector) <= 1)
                {
                    var direction = DirectionMethods.NearestDirectionFromVector(relativeVector);
                    if (direction.HasValue)
                        character.Turn(direction.Value);
                }
            }

            var actions = new List<IAction>();
            if (!result.IsDiscoveringCharacter())
            {
                actions.AddRange(GenerateMoveActionsDoable(character, result, map));
            }
            else if (PrioritizeMovement(character, result.TargetPosition))
            {
                Log.Debug($"[Think] Prioritize Movement.");
                actions.AddRange(GenerateMoveActionsDoable(character, result, map));
                if (!actions.Any(action => action.Evaluate(character, map) > 0))
                {
                    actions.AddRange(GenerateUseSkillActionsDoable(character, map));
                    actions.AddRange(GenerateUseItemActionsDoable(character, map));
                    actions.AddRange(GenerateThrowItemActionsDoable(character, map));
                }
            }
            else
            {
                Log.Debug($"[Think] Not Prioritize Movement.");
                actions.AddRange(GenerateUseSkillActionsDoable(character, map));
                actions.AddRange(GenerateUseItemActionsDoable(character, map));
                actions.AddRange(GenerateThrowItemActionsDoable(character, map));
                if (!actions.Any(action => action.Evaluate(character, map) > 0))
                {
                    actions.AddRange(GenerateMoveActionsDoable(character, result, map));
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

            _previousResult = result;
            return action;
        }

        private BehaviorResult GenerateNextBehaviorResult(IHasBehavior character, IMap map)
        {
            var visibleEnemies = map.Characters.FromAffiliation(character, AffiliationType.Enemy).IsVisible(character.Entity.CurrentPosition);
            var visibleLeaders = map.Characters.Where(c => c.IsLeader).FromAffiliation(character, AffiliationType.Ally).IsVisible(character.Entity.CurrentPosition);
            if (character.IsAlly(map.Player))
            {
                visibleLeaders = visibleLeaders.Append(map.Player);
            }

            var targetedEnemy = visibleEnemies.MinByOrDefault(enemy => VectorExtension.ChebyshevDistance(character.Entity.CurrentPosition, enemy.Entity.CurrentPosition), null);
            var targetedLeader = visibleLeaders.MaxByOrDefault(leader => character.Affiliation.GetAffection(leader.Affiliation), null);

            if (BehaviorData.PrioritizeEnemiesOverLeaders)
            {
                if (targetedEnemy != null)
                {
                    return new BehaviorResult(BehaviorState.DiscoveringEnemy, targetedEnemy.Entity.CurrentPosition);
                }
                else if (targetedLeader != null)
                {
                    return new BehaviorResult(BehaviorState.DiscoveringLeader, targetedLeader.Entity.CurrentPosition);
                }
            }
            else
            {
                if (targetedLeader != null)
                {
                    return new BehaviorResult(BehaviorState.DiscoveringLeader, targetedLeader.Entity.CurrentPosition);
                }
                else if (targetedEnemy != null)
                {
                    return new BehaviorResult(BehaviorState.DiscoveringEnemy, targetedEnemy.Entity.CurrentPosition);
                }
            }

            if (_previousResult.State == BehaviorState.ApproachingToObserve)
            {
                if (CanReachButNotAtTarget(character, _previousResult.TargetPosition.Value, map)
                && !character.VisionRange.IsVisible(_previousResult.TargetPosition.Value))
                {
                    return _previousResult;
                }
            }
            else if (_homePosition.HasValue)
            {
                return new BehaviorResult(BehaviorState.ReturningHome, _homePosition.Value.Position);
            }

            if (_previousResult.State == BehaviorState.MovingToLastKnownEnemyPosition
            && CanReachButNotAtTarget(character, _previousResult.TargetPosition.Value, map))
            {
                return _previousResult;
            }
            else if (_previousResult.State == BehaviorState.DiscoveringEnemy
            && IsChasingEnemy()
            && CanReachButNotAtTarget(character, _previousResult.TargetPosition.Value, map))
            {
                return new BehaviorResult(BehaviorState.MovingToLastKnownEnemyPosition, _previousResult.TargetPosition);
            }
            else if (_previousResult.State == BehaviorState.MovingToLastKnownLeaderPosition
            && CanReachButNotAtTarget(character, _previousResult.TargetPosition.Value, map))
            {
                return _previousResult;
            }
            else if (_previousResult.State == BehaviorState.DiscoveringLeader
            && CanReachButNotAtTarget(character, _previousResult.TargetPosition.Value, map))
            {
                return new BehaviorResult(BehaviorState.MovingToLastKnownLeaderPosition, _previousResult.TargetPosition);
            }

            return new BehaviorResult(BehaviorState.Wandering, null);
        }

        public bool CanReachButNotAtTarget(IHasBehavior character, Vector2Int targetPosition, IMap map)
        {
            return character.Entity.CurrentPosition != targetPosition
                   && map.IsReachable(character.Entity.CurrentPosition, targetPosition, character);
        }

        public bool IsChasingEnemy()
        {
            return _default is Chase && (_greaterThanTopBound == null || _greaterThanTopBound is Chase) && (_lessThanBottomBound == null || _lessThanBottomBound is Chase);
        }

        private float GetDistance(IHasBehavior character, Vector2Int targetPosition)
        {
            var distance = VectorExtension.ChebyshevDistance(character.Entity.CurrentPosition, targetPosition);
            return distance;
        }

        public IBehaviorWhenDiscoveringTarget GetDiscoveredTargetBehavior(IHasBehavior character,
            Vector2Int targetPosition, BehaviorState state)
        {
            switch (state)
            {
                case BehaviorState.DiscoveringLeader:
                    return _discoveringLeader;
                case BehaviorState.ReturningHome:
                    return _returningHome;
                case BehaviorState.ApproachingToObserve:
                    return new Chase();
                case BehaviorState.DiscoveringEnemy:
                    var distance = GetDistance(character, targetPosition);
                    if (_greaterThanTopBound != null && distance > _distanceTopBound)
                    {
                        Log.Debug($"[Think] Distance is greater than top bound {_distanceTopBound}.");
                        return _greaterThanTopBound;
                    }
                    if (_lessThanBottomBound != null && distance < _distanceBottomBound)
                    {
                        Log.Debug($"[Think] Distance is less than bottom bound {_distanceBottomBound}.");
                        return _lessThanBottomBound;
                    }
                    Log.Debug("[Think] Behavior is Default.");
                    return _default;
                case BehaviorState.MovingToLastKnownEnemyPosition:
                case BehaviorState.MovingToLastKnownLeaderPosition:
                    return new Chase();
                case BehaviorState.Wandering:
                    throw new Exception("Wandering is not a valid state for discovered target behavior.");
                default:
                    throw new Exception($"Invalid state: {state}");
            }


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

        private IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, BehaviorResult result,
            IMap map)
        {
            if (result.TargetPosition != null)
            {
                return GetDiscoveredTargetBehavior(character, result.TargetPosition.Value, result.State)
                    .GenerateMoveActionsDoable(character, result.TargetPosition.Value, map);
            }

            return _wander.GenerateMoveActionsDoable(character, map);
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap map)
        {
            var actions = new List<UseSkill>();
            foreach (var skill in character.Skills)
            {
                if (skill.IsDirectional)
                {
                    actions.AddRange(DirectionMethods.AllDirections.Select(direction => new UseSkill(skill, direction)));
                }
                else
                {
                    actions.Add(new UseSkill(skill, character.CurrentDirection));
                }
            }
            return actions.Where(action => action.Doable(character, map));
        }

        private IEnumerable<UseItem> GenerateUseItemActionsDoable(IHasBehavior character, IMap map)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<UseItem>();
            }

            var actions = new List<UseItem>();
            foreach (var item in character.Inventory.AllItems)
            {
                if (!item.CanActivateWhenUsed)
                    continue;

                if (item.SkillOnUse.MapOr(false, skill => skill.IsDirectional))
                {
                    actions.AddRange(DirectionMethods.AllDirections.Select(direction => new UseItem(item, direction)));
                }
                else
                {
                    actions.Add(new UseItem(item, character.CurrentDirection));
                }
            }
            return actions.Where(action => action.Doable(character, map));
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

        public void KnowLocationOf(Vector2Int position)
        {
            _previousResult = new BehaviorResult(
                BehaviorState.ApproachingToObserve,
                position
            );
        }

        public UniTask<IItem?> SelectItem(IInventory inventory, IMap map, params int[] disabledItemIds)
        {
            return UniTask.FromResult<IItem?>(null);
        }
    }
}