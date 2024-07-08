using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class RepairEffect : IEffect
    {
        public Color Color => Colors.Brown;

        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            target.RepairAllItem();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"修復(手持ち全て)";
        }
    }
}