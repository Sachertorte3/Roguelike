using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class TeleportEffect : IEffect
    {
        public Impact Impact => Impact.Neutral;
        public Color Color => Colors.SkyBlue;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            var position = map.GetAllPassablePositions().GetAtRandom();
            target.Teleport(position);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return "テレポート";
        }
    }
}