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
        [MinValue(1), SerializeField] private int _power;

        public HealEffect(int power)
        {
            _power = power;
        }

        public Color Color => Colors.Green;

        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            var value = Formula.Calc(actor, _power);
            GameLog.Add($"{target.GetName(map.Player)}は{value}回復");
            target.GainHp(value);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1,
                Mathf.Min(target.CurrentMaxHp - target.CurrentHp, (float)Formula.Calc(actor, _power)) /
                target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"回復\n威力: {_power}";
        }
    }
}