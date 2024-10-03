using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
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

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            var value = Formula.CalcHeal(_power);
            GameLog.Add($"{target.GetName(map.Player)}は{value}回復");
            target.GainHp(value);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var lostRatio = (float)(target.CurrentMaxHp - target.CurrentHp) / target.CurrentMaxHp;
            var healRatio = (float)Formula.CalcHeal(_power) / target.CurrentMaxHp;
            if (lostRatio >= healRatio)
            {
                return healRatio;
            }
            else if (lostRatio > 0.5f)
            {
                return lostRatio;
            }
            else
            {
                return 0;
            }
        }

        public float EvaluatePrice()
        {
            return Formula.EvaluateHeal(_power);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new()
        {
            { new UpgradePath("回復量"), new UpgradeData("回復量+3", () => _power += 3) }
        };

        public string Info()
        {
            return $"回復\n威力: {_power}";
        }
    }
}