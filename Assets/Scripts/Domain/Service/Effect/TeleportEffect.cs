using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Setting;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class TeleportEffect : ActorlessEntityTargetEffect
    {
        public override Impact Impact => Impact.Neutral;
        public override Color Color => Colors.SkyBlue;

        public override async UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            var randomPosition = map.GetAllBlankAndStandablePositionsOn(EntityLayer.Middle).GetAtRandom();
            target.Teleport(randomPosition);
            await UniTask.Delay(Settings.MoveMilliseconds.CurrentValue);
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Info()
        {
            return "テレポート";
        }
    }
}