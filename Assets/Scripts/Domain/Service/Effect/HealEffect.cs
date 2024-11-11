using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class HealEffect : EntityTargetEffect
    {
        [MinValue(1)] [SerializeField] private int _power;

        public override Color Color => Colors.Green;

        public override Impact Impact => Impact.Beneficial;

        public override UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var value = Formula.CalcHeal(_power);
            GameLog.Add($"{target.GetName(map.Player)}は{value}回復");
            target.GainHp(value);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var lostRatio = (float)(target.CurrentMaxHp - target.CurrentHp) / target.CurrentMaxHp;
            var healRatio = (float)Formula.CalcHeal(_power) / target.CurrentMaxHp;
            if (lostRatio >= healRatio)
            {
                return healRatio;
            }

            if (lostRatio > 0.5f)
            {
                return lostRatio;
            }

            return 0;
        }

        public override float EvaluatePrice()
        {
            return Formula.EvaluateHeal(_power);
        }

        public override string UpgradePathName => "回復";

        public override List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>
            {
                new(
                    "回復量+3",
                    () => _power += 3,
                    () => _power -= 3
                )
            };
        }

        public override Dictionary<string, IHasUpgrades> GetChildren()
        {
            return new Dictionary<string, IHasUpgrades>();
        }

        public override string Info()
        {
            return $"威力{_power}の回復を行う\n";
        }
    }
}