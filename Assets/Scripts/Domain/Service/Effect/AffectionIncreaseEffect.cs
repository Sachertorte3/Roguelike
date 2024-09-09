using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AffectionIncreaseEffect : IEffect
    {
        [MinValue(1), SerializeField] private float _power;

        public AffectionIncreaseEffect(float power)
        {
            _power = power;
        }

        public Color Color => Colors.HotPink;

        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return _power;
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return $"好感度上昇\n威力: {_power}";
        }
    }
}