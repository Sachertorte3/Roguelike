using System;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public record SkillData : IHasInfo
    {
        [SerializeReference][Required] public IArea Area;
        [SerializeReference][Required] public IEffect Effect;
        [SerializeReference][Required] public IEffectPosition Position = new AtFeet();
        [Required] public string Log = "は行動した";

        public SkillData(IEffectPosition position, IArea area, IEffect effect, string log)
        {
            Position = position;
            Area = area;
            Effect = effect;
            Log = log;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}";
        }
    }
}