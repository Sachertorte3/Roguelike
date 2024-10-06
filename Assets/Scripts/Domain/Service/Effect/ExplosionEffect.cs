using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ExplosionEffect : IActorlessEffect
    {
        [SerializeField]
        [Range(0, 1)] private float _damageRate = 0.5f;

        [SerializeField] private bool _canDigWall = false;
        [SerializeField] private List<AdditionalConditionData> _additionalConditions = new();

        public ExplosionEffect(float damageRate, bool canDigWall, List<AdditionalConditionData> additionalConditions)
        {
            _damageRate = damageRate;
            _canDigWall = canDigWall;
            _additionalConditions = additionalConditions;
        }

        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) => Apply(target, map);
        public async UniTask Apply(ITargetOfEffect target, IMap map)
        {
            var damage = Formula.CalcExplosionDamage(_damageRate, target);
            GameLog.Add($"{target.GetName(map.Player)}に{damage}のダメージ");
            target.LoseHp(damage);

            foreach (var condition in _additionalConditions)
            {
                if (Random.value < condition.Probability)
                {
                    target.AddCondition(Id<IEntity>.Empty, condition.Condition.Value.Condition,
                        condition.Condition.Value.RemovalCondition);
                }
            }
        }

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map) => Apply(positions, map);
        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            if (_canDigWall)
            {
                map.RemoveWalls(positions);
            }

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.CalcExplosionDamage(_damageRate, target)) /
                             target.CurrentMaxHp);
            result += _additionalConditions.Sum(condition =>
                condition.Probability * condition.Condition.Value.Evaluate(target));
            return result;
        }

        public float EvaluatePrice()
        {
            var result = (float)Formula.EvaluateExplosionDamage(_damageRate);
            result += _additionalConditions.Sum(condition =>
                condition.Probability * condition.Condition.Value.EvaluateDamage());
            return result;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            var info = $"攻撃\n威力: HPの{_damageRate:P0}";

            if (_additionalConditions.Count > 0)
            {
                info += "\n追加状態付与:";
                foreach (var condition in _additionalConditions)
                {
                    info += $"\n{condition.Info()}";
                }
            }

            return info;
        }
    }
}