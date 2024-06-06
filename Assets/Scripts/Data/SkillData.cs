using System;
using Data.Area;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [Serializable]
    public record SkillData : IHasInfo
    {
        [SerializeReference, Required] public IArea Area;
        [SerializeReference, Required] public IEffect Effect;

        public SkillData(IArea area, IEffect effect)
        {
            Area = area;
            Effect = effect;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}";
        }
    }
}