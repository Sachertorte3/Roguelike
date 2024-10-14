using System;
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
    public class BreakEffect : ActorlessEntityTargetEffect
    {
        public override Color Color => Colors.Black;
        public override Impact Impact => Impact.Harmful;
        public bool OnlyApplyToItem = false;

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
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

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public override float EvaluatePrice()
        {
            return 100f;
        }

        public override string Info()
        {
            return "破壊";
        }
    }
}