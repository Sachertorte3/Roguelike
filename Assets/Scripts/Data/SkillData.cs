using Data.Area;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public record SkillData : IHasInfo
    {
        [SerializeReference, Required] public IEffect Effect;
        [SerializeReference, Required] public IArea Area;

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