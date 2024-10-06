using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ExplosionEffect : IActorlessEffect
    {
        [SerializeField]
        [Range(0, 1)] private float _damageRate = 0.5f;

        public ExplosionEffect(float damageRate)
        {
            _damageRate = damageRate;
        }

        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) => Apply(target, map);
        public UniTask Apply(ITargetOfEffect target, IMap map)
        {
            var damage = Formula.CalcExplosionDamage(_damageRate, target);
            GameLog.Add($"{target.GetName(map.Player)}に{damage}のダメージ");
            target.LoseHp(damage);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.CalcExplosionDamage(_damageRate, target)) /
                             target.CurrentMaxHp);
            return result;
        }

        public float EvaluatePrice()
        {
            return Formula.EvaluateExplosionDamage(_damageRate);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return $"攻撃\n威力: HPの{_damageRate:P0}";
        }
    }
}