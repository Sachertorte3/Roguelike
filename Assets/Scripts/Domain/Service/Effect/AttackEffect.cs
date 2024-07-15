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
        [MinValue(1)] public int Power;
        [Range(0, 1)] public float CriticalRate;
        public List<AdditionalConditionData> AdditionalConditions = new();

        public AttackEffect(int power, List<AdditionalConditionData> additionalConditions)
        {
            Power = power;
            AdditionalConditions = additionalConditions;
        }

        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            if (Random.value < CriticalRate)
            {
                GameLog.Add($"<color=red>クリティカル！{target.GetName(map.Player)}に{Power * 2}のダメージ</color>");
                await target.LoseHp(Formula.Calc(actor, Power * 2));
            }
            else
            {
                GameLog.Add($"{target.GetName(map.Player)}に{Power}のダメージ");
                await target.LoseHp(Formula.Calc(actor, Power));
            }
            foreach (var condition in AdditionalConditions)
            {
                if (Random.value < condition.Probability)
                {
                    target.AddCondition(condition.Condition, condition.RemovalCondition);
                }
            }
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, Power)) / target.CurrentMaxHp);
        }

        public string Info()
        {
            var info = $"攻撃\n威力: {Power}";
            if (AdditionalConditions.Count > 0)
            {
                info += "\n追加状態付与:";
                foreach (var condition in AdditionalConditions)
                {
                    info += $"\n{condition.Info()}";
                }
            }

            return info;
        }
    }
}