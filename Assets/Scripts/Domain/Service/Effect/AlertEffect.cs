using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AlertEffect : IActorlessEffect
    {
        public Color Color => Colors.Red;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            target.ListenToAlert(actor);

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.2f;
        }

        public float EvaluatePrice()
        {
            return 20;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"警報";
        }
    }
    [Serializable]
    public class ForgetEffect : IActorlessEffect
    {
        public Color Color => Colors.White;

        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) => Apply(target, map);
        public UniTask Apply(ITargetOfEffect target, IMap map)
        {
            target.ClearKnownItems(map);
            target.ClearAffiliation(map);

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.2f;
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"忘却";
        }
    }
}