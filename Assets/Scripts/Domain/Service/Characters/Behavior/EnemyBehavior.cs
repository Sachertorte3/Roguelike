#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Logging;
using Utilities;
using Domain.Model.Characters;
using Domain.Model.Action;
using Domain.Model;

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

            if (_lastTarget != null) //ターゲットがいる
            {
                if (visibleCharacters.Contains(_lastTarget)) //ターゲットは視界内である
                {
                    if (character.IsEnemy(_lastTarget)) //ターゲットは敵である
                    {
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else if (character.IsAlly(_lastTarget) && _lastTarget.IsLeader) //ターゲットは味方かつリーダーである
                    {
                        if (visibleEnemies.Any()) //敵がいる
                        {
                            _lastTarget = visibleEnemies.First();
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                        else
                        {
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                    }
                    else //ターゲットはいるが敵でも味方でもない
                    {
                        Log.Debug(
                            $"[Think] Stopped targeting because the target {_lastTarget.GetName(world.Player)} is neither friend nor enemy.");
                        if (visibleEnemies.Any()) //他に敵がいる
                        {
                            Log.Debug($"[Think] Change target to Enemy {_lastTarget.GetName(world.Player)}.");
                            _lastTarget = visibleEnemies.First();
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                        else if (visibleLeaders.Any()) //他にリーダーがいる
                        {
                            Log.Debug($"[Think] Change target to Leader {_lastTarget.GetName(world.Player)}.");
                            _lastTarget = visibleLeaders.First();
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                        else //他に敵もリーダーもいない
                        {
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                    }
                }
                else //ターゲットを見失った
                {
                    Log.Debug($"[Think] Target {_lastTarget.GetName(world.Player)} is out of sight.");
                    if (visibleEnemies.Any()) //他に敵がいる
                    {
                        _lastTarget = visibleEnemies.First();
                        Log.Debug($"[Think] Change target to Enemy {_lastTarget.GetName(world.Player)}.");
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else if (visibleLeaders.Any()) //他にリーダーがいる
                    {
                        _lastTarget = visibleLeaders.First();
                        Log.Debug($"[Think] Change target to Leader {_lastTarget.GetName(world.Player)}.");
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else //他に敵もリーダーもいない
                    {
                        if (character.CurrentPosition == _lastTargetPosition) //ターゲットの最後にいた座標にいる
                        {
                            Log.Debug($"[Think] Abandoned pursuit of target {_lastTarget.GetName(world.Player)}.");
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                        else if (!world.IsReachable(character.CurrentPosition,
                                     _lastTarget.CurrentPosition)) //ターゲットの最後にいた座標にはたどり着けない
                        {
                            Log.Debug($"[Think] Abandoned pursuit of target {_lastTarget.GetName(world.Player)}.");
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                    }
                }
            }
            else //ターゲットはいない
            {
                if (visibleEnemies.Any()) //敵がいる
                {
                    _lastTarget = visibleEnemies.First();
                    Log.Debug($"[Think] Discover Enemy {_lastTarget.GetName(world.Player)}.");
                    _lastTargetPosition = _lastTarget.CurrentPosition;
                }
                else if (visibleLeaders.Any()) //他にリーダーがいる
                {
                    _lastTarget = visibleLeaders.First();
                    Log.Debug($"[Think] Discover Leader {_lastTarget.GetName(world.Player)}.");
                    _lastTargetPosition = _lastTarget.CurrentPosition;
                }
                else //敵はいない
                {
                    _lastTarget = null;
                    _lastTargetPosition = null;
                }
            }

            if (_lastTargetPosition != null) //目指す座標がある
            {
                Log.Debug($"[Think] Target exists.");
                var actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value, world);

                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0);
                foreach (var actionTemp in validActions)
                {
                    Log.Debug($"[Think] {actionTemp.Info()} {actionTemp.Evaluate(character, world)}");
                }

                var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                    action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness),
                    new DoNothing()));
                return action;
            }
            else
            {
                Log.Debug($"[Think] Wandering around.");
                var actions = _wander.GenerateActionsDoable(character, world);

                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0);
                foreach (var actionTemp in validActions)
                {
                    Log.Debug($"[Think] {actionTemp.Info()} {actionTemp.Evaluate(character, world)}");
                }

                var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                    action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness),
                    new DoNothing()));
                return action;
            }
        }
    }
}