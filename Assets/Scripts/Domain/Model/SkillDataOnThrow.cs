using System;
using Domain.Model.Area;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    [Serializable]
    public class SkillDataOnThrow : IHasInfo
    {
        [SerializeReference, Required] public IArea Area;
        [SerializeReference, Required] public IEffect Effect;

        public SkillDataOnThrow(IArea area, IEffect effect)
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