#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Logging;
using Utilities;
using Domain.Model.Character;
using Domain.Model.Action;
using Domain.Model;
using Domain.Service.Action;
using Domain.Model.Item;
using R3;

namespace Domain.Service.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        public Observable<OnItemSelectMessage> OnItemSelect { get; init; } = new Subject<OnItemSelectMessage>();
        private readonly IBehaviorWhenUndiscoveringTarget _wander;
        private readonly float behavioralRandomness = 0.01f;
        private ICharacter? _lastTarget;
        private Vector2Int? _lastTargetPosition;

        private readonly IBehaviorWhenDiscoveringTarget _default;
        private readonly bool _prioritizeMovement = false;
        private readonly float _distanceTopBound = float.PositiveInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _greaterThanTopBound = null;
        private readonly bool _prioritizeMovementWhenDistanceGreaterThanTopBound = false;
        private readonly float _distanceBottomBound = float.NegativeInfinity;
        private readonly IBehaviorWhenDiscoveringTarget? _lessThanBottomBound = null;
        private readonly bool _prioritizeMovementWhenDistanceLessThanBottomBound = false;
        public BehaviorData BehaviorData { get; init; }

        public EnemyBehavior(BehaviorData data)
        {
            BehaviorData = data;
            if (data.wanderAround)
            {
                _wander = new Wander();
            }
            else
            {
                _wander = new NoMove();
            }
            _default = data.Default.ToDiscoveredTargetBehavior();
            _prioritizeMovement = data.PrioritizeMovement;
            if (data.UseTopBound)
            {
                _distanceTopBound = data.distanceTopBound;
                _greaterThanTopBound = data.greaterThanTopBound.ToDiscoveredTargetBehavior();
                _prioritizeMovementWhenDistanceGreaterThanTopBound = data.PrioritizeMovementWhenDistanceGreaterThanTopBound;
            }
            if (data.UseBottomBound)
            {
                _distanceBottomBound = data.distanceBottomBound;
                _lessThanBottomBound = data.lessThanBottomBound.ToDiscoveredTargetBehavior();
                _prioritizeMovementWhenDistanceLessThanBottomBound = data.PrioritizeMovementWhenDistanceLessThanBottomBound;
            }
        }

        public bool WanderAround { get; init; }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            HashSet<Vector2Int> visibleArea = new(character.VisionRange.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);

            var visibleCharacters = world.GetVisibleCharacters(character);
            var visibleEnemies = visibleCharacters.Where(c => character.IsEnemy(c));
            var visibleLeaders = visibleCharacters.Where(c => character.IsAlly(c) && c.IsLeader);

            var targetedEnemy = GetTargetedEnemy(character, visibleEnemies, world);
            var targetedLeader = GetTargetedLeader(character, visibleLeaders, world);
            if (targetedEnemy != null)
            {
                Log.Debug($"[Think] Discover Enemy {targetedEnemy.GetName(world.Player)}.");
                _lastTarget = targetedEnemy;
                _lastTargetPosition = targetedEnemy.CurrentPosition;
            }
            else if (targetedLeader != null)
            {
                Log.Debug($"[Think] Discover Leader {targetedLeader.GetName(world.Player)}.");
                _lastTarget = targetedLeader;
                _lastTargetPosition = targetedLeader.CurrentPosition;
            }
            else if (_lastTargetPosition.HasValue)
            {
                _lastTargetPosition = GetTargetPosition(character, world);
            }

            if (_lastTargetPosition != null) //目指す座標がある
            {
                Log.Debug($"[Think] Target position is {_lastTargetPosition}.");
            }
            else
            {
                Log.Debug($"[Think] Wandering around.");
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

        public ICharacter? GetTargetedEnemy(IHasBehavior character, IEnumerable<ICharacter> visibleEnemies, IMap map)
        {
            if (visibleEnemies.Contains(_lastTarget))
                return _lastTarget;
            return visibleEnemies.FirstOrDefault();
        }

        public ICharacter? GetTargetedLeader(IHasBehavior character, IEnumerable<ICharacter> visibleLeaders, IMap map)
        {
            if (visibleLeaders.Contains(_lastTarget))
                return _lastTarget;
            return visibleLeaders.FirstOrDefault();
        }

        public Vector2Int? GetTargetPosition(IHasBehavior character, IMap world)
        {
            return
                _lastTargetPosition != null
                && character.CurrentPosition != _lastTargetPosition
                && world.IsReachable(character.CurrentPosition, _lastTargetPosition.Value) ?
                    _lastTargetPosition : null;
        }

            private int GetDistance(IHasBehavior character, Vector2Int targetPosition)
        {
            var distance = Mathf.Max(Mathf.Abs(character.CurrentPosition.x - targetPosition.x), Mathf.Abs(character.CurrentPosition.y - targetPosition.y));
            return distance;
        }

        public IBehaviorWhenDiscoveringTarget GetDiscoveredTargetBehavior(IHasBehavior character, Vector2Int targetPosition)
        {
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