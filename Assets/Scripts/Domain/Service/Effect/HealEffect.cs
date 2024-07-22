using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class HealEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public HealEffect(int power)
        {
            Power = power;
        }

        public Color Color => Colors.Green;

        public Impact Impact => Impact.Beneficial;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            var value = Formula.Calc(actor, Power);
            GameLog.Add($"{target.GetName(map.Player)}は{value}回復");
            await target.GainHp(value);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1,
                Mathf.Min(target.CurrentMaxHp - target.CurrentHp, (float)Formula.Calc(actor, Power)) /
                target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"回復\n威力: {Power}";
        }
    }
}