using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Characters;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BreakEffect : IActorlessEffect
    {
        public Color Color => Colors.Black;
        public Impact Impact => Impact.Harmful;
        public bool OnlyApplyToItem = false;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) => Apply((IEntity)target, map);

        public UniTask Apply(IActorOfEffect actor, IEntity target, IMap map) => Apply(target, map);

        public UniTask Apply(ITargetOfEffect target, IMap map) => Apply((IEntity)target, map);

        public UniTask Apply(IEntity target, IMap map)
        {
            if (!OnlyApplyToItem || target is ItemEntity)
            {
                if (target is Character character)
                {
                    GameLog.Add($"{character.GetName(map.Player)}は破壊された");
                }
                else if (target is ItemEntity item)
                {
                    GameLog.Add($"{item.Item.GetName(map.Player, map.ItemDatabase)}は破壊された");
                }
                target.Destroy();
            }
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

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "破壊";
        }
    }
}