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
    public class PercentageDamageEffect : ActorlessEntityTargetEffect
    {
        [SerializeField]
        [Range(0, 1)] private float _damageRate = 0.5f;

        public PercentageDamageEffect(float damageRate)
        {
            _damageRate = damageRate;
        }

        public override Color Color => Colors.Red;

        public override Impact Impact => Impact.Harmful;

        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var damage = Formula.CalcExplosionDamage(_damageRate, target);
            GameLog.Add($"{target.GetName(map.Player)}に{damage}のダメージ");
            target.LoseHp(damage);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.CalcExplosionDamage(_damageRate, target)) /
                             target.CurrentMaxHp);
            return result;
        }

        public override float EvaluatePrice()
        {
            return Formula.EvaluateExplosionDamage(_damageRate);
        }

        public override Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public override string Info()
        {
            return $"攻撃\n威力: HPの{_damageRate:P0}";
        }
    }
}