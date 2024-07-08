using System;
using Domain.Model.Area;
using Domain.Model.Effect;
using Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    [Serializable]
    public class SkillDataOnUse : IHasInfo
    {
        [SerializeReference] [Required] public IArea Area;
        [SerializeReference] [Required] public IEffect Effect;
        [SerializeReference] [Required] public IEffectPosition Position;

        public SkillDataOnUse(IEffectPosition position, IArea area, IEffect effect)
        {
            Position = position;
            Area = area;
            Effect = effect;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}";
        }
    }
}