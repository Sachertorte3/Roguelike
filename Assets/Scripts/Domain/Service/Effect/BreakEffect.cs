using System;
using Cysharp.Threading.Tasks;
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

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.LoseHp(target.CurrentHp);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"破壊";
        }
    }
}