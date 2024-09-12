using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BreakEffect : IEffect
    {
        public Color Color => Colors.Black;
        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, IEntity target, IMap map)
        {
            target.Destroy();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public float EvaluatePrice()
        {
            return 100f;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return $"破壊";
        }
    }
}