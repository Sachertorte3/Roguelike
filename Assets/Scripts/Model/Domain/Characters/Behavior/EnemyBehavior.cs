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
        private readonly IUndiscoveredTargetBehavior _wander = new RandomWalk();
        private readonly float behavioralRandomness = 0.01f;
        private Character? _lastTarget;
        private Vector2Int? _lastTargetPosition;

        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            HashSet<Vector2Int> visibleArea = new(character.Area.VisibleArea);
            visibleArea.Remove(character.CurrentPosition);
            var visibleCharacters = world.GetVisibleCharacters(character);
            var visibleEnemies = visibleCharacters.Where(c => character.Affiliation.IsEnemy(c.Affiliation));

            if (_lastTarget != null)//ターゲットがいる
            {
                if (visibleCharacters.Contains(_lastTarget))//ターゲットは視界内である
                {
                    if (character.Affiliation.IsEnemy(_lastTarget.Affiliation))//ターゲットは敵である
                    {
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else//ターゲットはいるが敵ではない
                    {
                        if (visibleEnemies.Any())//他に敵がいる
                        {
                            _lastTarget = visibleEnemies.First();
                            _lastTargetPosition = _lastTarget.CurrentPosition;
                        }
                        else//他に敵はいない
                        {
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                    }
                }
                else//ターゲットを見失った
                {
                    if (visibleEnemies.Any())//他に敵がいる
                    {
                        _lastTarget = visibleEnemies.First();
                        _lastTargetPosition = _lastTarget.CurrentPosition;
                    }
                    else//他に敵はいない
                    {
                        if (character.CurrentPosition == _lastTargetPosition)//ターゲットの最後にいた座標にいる
                        {
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                        else if (!world.IsReachable(character.CurrentPosition, _lastTarget.CurrentPosition))//ターゲットの最後にいた座標にはたどり着けない
                        {
                            _lastTarget = null;
                            _lastTargetPosition = null;
                        }
                    }
                }
            }
            else//ターゲットはいない
            {
                if (visibleEnemies.Any())//敵がいる
                {
                    _lastTarget = visibleEnemies.First();
                    _lastTargetPosition = _lastTarget.CurrentPosition;
                }
                else//敵はいない
                {
                    _lastTarget = null;
                    _lastTargetPosition = null;
                }
            }

            Debug.Log(_lastTarget);
            Debug.Log(_lastTargetPosition);

            if (_lastTargetPosition != null)//目指す座標がある
            {
                var actions = _chase.GenerateActionsDoable(character, _lastTargetPosition.Value, world);
                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0).ToList();
                return UniTask.FromResult(validActions.MaxByOrDefault(action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness), new DoNothing()));
            }
            else
            {
                var actions = _wander.GenerateActionsDoable(character, world);
                var validActions = actions.Where(action => action.Evaluate(character, world) >= 0).ToList();
                return UniTask.FromResult(validActions.MaxByOrDefault(action => action.Evaluate(character, world) + Random.Range(0, behavioralRandomness), new DoNothing()));
            }
        }
    }
}