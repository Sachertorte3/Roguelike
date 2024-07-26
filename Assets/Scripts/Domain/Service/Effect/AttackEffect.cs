using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [MinValue(1), SerializeField] private int _power;
        [Range(0, 1), SerializeField] private float _criticalRate;
        [SerializeField] private int _blowAwayDistance;
        [SerializeField] private List<AdditionalConditionData> _additionalConditions = new();

        public AttackEffect(int power, float criticalRate, List<AdditionalConditionData> additionalConditions, int blowAwayDistance)
        {
            _power = power;
            _criticalRate = criticalRate;
            _additionalConditions = additionalConditions;
            _blowAwayDistance = blowAwayDistance;
        }

        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            if (Random.value < _criticalRate)
            {
                var damage = Formula.Calc(actor, _power * 2);
                GameLog.Add($"<color=red>クリティカル！{target.GetName(map.Player)}に{damage}のダメージ</color>");
                await target.LoseHp(damage);
            }
            else
            {
                var damage = Formula.Calc(actor, _power);
                GameLog.Add($"{target.GetName(map.Player)}に{damage}のダメージ");
                await target.LoseHp(damage);
            }
            foreach (var condition in _additionalConditions)
            {
                if (Random.value < condition.Probability)
                {
                    target.AddCondition(condition.Condition.Condition, condition.Condition.RemovalCondition);
                }
            }
            if (_blowAwayDistance > 0)
            {
                await target.BlowAway(DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition).Value, _blowAwayDistance, map);
            }
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, _power)) / target.CurrentMaxHp);
        }

        public string Info()
        {
            var info = $"攻撃\n威力: {_power}";
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