using System;
using System.Collections.Generic;
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
    public class TeleportEffect : IActorlessEffect
    {
        public Impact Impact => Impact.Neutral;
        public Color Color => Colors.SkyBlue;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            return Apply((IEntity)target, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEntity target, IMap map)
        {
            return Apply(target, map);
        }

        public UniTask Apply(ITargetOfEffect target, IMap map)
        {
            return Apply((IEntity)target, map);
        }

        public async UniTask Apply(IEntity target, IMap map)
        {
            var position = map.GetAllBlankAndStandablePositionsOn(EntityLayer.Middle).GetAtRandom();
            target.Teleport(position);
            await UniTask.Delay(Settings.MoveMilliseconds.CurrentValue);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public float EvaluatePrice()
        {
            return 50f;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "テレポート";
        }
    }
}