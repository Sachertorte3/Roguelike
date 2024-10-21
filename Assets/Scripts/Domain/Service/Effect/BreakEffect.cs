using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Characters;
using Domain.Service.Events;
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
        public bool ApplyToCharacter = true;
        public bool ApplyToItem = true;
        public bool ApplyToMoney = true;
        public bool ApplyToTrap = true;
        public bool ApplyToChest = true;

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            if (target is Character character && ApplyToCharacter)
            {
                GameLog.Add($"{character.GetName(map.Player)}は破壊された");
            }
            else if (target is ItemEntity item && ApplyToItem)
            {
                GameLog.Add($"{item.Item.GetName(map.Player, map.ItemPlaceholders)}は破壊された");
            }
            else if (target is Money money && ApplyToMoney)
            {
                GameLog.Add($"{money.Amount}Gは破壊された");
            }
            else if (target is Trap trap && ApplyToTrap)
            {
                GameLog.Add($"{trap.Name}は破壊された");
            }
            else if (target is Chest chest && ApplyToChest)
            {
                GameLog.Add($"宝箱は破壊された");
            }
            else
            {
                return UniTask.CompletedTask;
            }
            target.Destroy();
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