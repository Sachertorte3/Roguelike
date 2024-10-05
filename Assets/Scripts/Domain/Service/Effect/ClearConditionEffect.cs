using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class ClearConditionEffect : IActorlessEffect
    {
        public Color Color => Colors.LightSkyBlue;
        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            return Apply(target, map);
        }

        public UniTask Apply(ITargetOfEffect target, IMap map)
        {
            target.ClearCondition();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public float EvaluatePrice()
        {
            return 500;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "全状態異常解除";
        }
    }
}