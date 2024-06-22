#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Domain.Action;
using UnityEngine;
using Utilities;

namespace Model.Domain.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        private readonly IDiscoveredTargetBehavior _chase = new Chase();
        private readonly IUndiscoveredTargetBehavior _wander = new Wander();
        private readonly float behavioralRandomness = 0.01f;
        private Character? _lastTarget;
        private Vector2Int? _lastTargetPosition;

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            HashSet<Vector2Int> visibleArea = new(character.Area.VisibleArea);
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
                        if (visibleEnemies.Any()) //他に敵がいる
                        {
                            _lastTarget = visibleEnemies.First();
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                        else if (visibleLeaders.Any()) //他にリーダーがいる
                        {
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
                    if (visibleEnemies.Any()) //他に敵がいる
                    {
                        _lastTarget = visibleEnemies.First();
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else if (visibleLeaders.Any()) //他にリーダーがいる
                    {
                        _lastTarget = visibleLeaders.First();
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else //他に敵もリーダーもいない
                    {
                        if (character.CurrentPosition == _lastTargetPosition) //ターゲットの最後にいた座標にいる
                        {
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                        else if (!world.IsReachable(character.CurrentPosition,
                                     _lastTarget.CurrentPosition)) //ターゲットの最後にいた座標にはたどり着けない
                        {
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
                    _lastTargetPosition = _lastTarget.CurrentPosition;
                }
                else if (visibleLeaders.Any()) //他にリーダーがいる
                {
                    _lastTarget = visibleLeaders.First();
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
                var actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value, world);
                foreach (var actionTemp in actions)
                {
                    Debug.Log($"{actionTemp.GetType()} {actionTemp.Evaluate(character, world)}");
                }

                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0).ToList();
                var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                    action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness),
                    new DoNothing()));
                Debug.Log($"{action.GetType()} {action.Evaluate(character, world)}");
                return action;
            }
            else
            {
                var actions = _wander.GenerateActionsDoable(character, world);
                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0).ToList();
                var action = await UniTask.FromResult(validActions.MaxByOrDefault(
                    action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness),
                    new DoNothing()));
                Debug.Log($"{action.GetType()} {action.Evaluate(character, world)}");
                return action;
            }
        }
    }
}