using System;
using Data.Area;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [Serializable]
    public record SkillData : IHasInfo
    {
        [MinValue(1)] public int Power;
        [SerializeReference] public IArea Area;

        public SkillData(int power, IArea area)
        {
            Power = power;
            Area = area;
        }

        public string Info()
        {
            return $"威力: {Power}\n範囲: {Area.Info()}";
        }
    }
}