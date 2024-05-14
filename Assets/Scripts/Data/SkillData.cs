using Scripts.Data.Area;
using System;
using UnityEngine;

namespace Scripts.Data
{
    [Serializable]
    public record SkillData: IHasInfo
    {
        public int Power;
        [SerializeReference] public IArea Area;
        public SkillData(int power, IArea area)
        {
            Power = power;
            Area = area;
        }
        public string Info() => $"威力: {Power}\n範囲: {Area.Info()}";
    }
}