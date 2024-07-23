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

namespace Domain.Service.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        private readonly IDiscoveredTargetBehavior _chase = new Chase();
        private readonly IUndiscoveredTargetBehavior _wander;
        private readonly float behavioralRandomness = 0.01f;
        private ICharacter? _lastTarget;
        private Vector2Int? _lastTargetPosition;

        public EnemyBehavior(bool wanderAround)
        {
            if (wanderAround)
                _wander = new Wander();
            else
                _wander = new NoMove();
            WanderAround = wanderAround;
        }

        public bool WanderAround { get; init; }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            HashSet<Vector2Int> visibleArea = new(character.VisionRange.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);

            var visibleCharacters = world.GetVisibleCharacters(character);
            var visibleEnemies = visibleCharacters.Where(c => character.IsEnemy(c));
            var visibleLeaders = visibleCharacters.Where(c => character.IsAlly(c) && c.IsLeader);

            var targetedEnemy = GetTargetedEnemy(visibleEnemies, world);
            var targetedLeader = GetTargetedLeader(visibleLeaders, world);
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

            var actions = GenerateActionsDoable(character, _lastTargetPosition, world);

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

        public ICharacter? GetTargetedEnemy(IEnumerable<ICharacter> visibleEnemies, IMap map)
        {
            if (visibleEnemies.Contains(_lastTarget))
                return _lastTarget;
            return visibleEnemies.FirstOrDefault();
        }

        public ICharacter? GetTargetedLeader(IEnumerable<ICharacter> visibleLeaders, IMap map)
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

        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int? targetPosition,
            IMap world)
        {
            var actions = new List<IAction>();
            if (targetPosition != null)
                actions.AddRange(_chase.GenerateMoveActionsDoable(character, targetPosition.Value, world));
            else
                actions.AddRange(_wander.GenerateMoveActionsDoable(character, world));
            actions.AddRange(GenerateUseSkillActionsDoable(character, world));
            actions.AddRange(GenerateUseItemActionsDoable(character, world));
            actions.AddRange(GenerateThrowItemActionsDoable(character, world));
            return actions;
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
    }
}