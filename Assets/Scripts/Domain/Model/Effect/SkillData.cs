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
        [SerializeField] public int RushDistance = 0;
        [Required] public string Log = "は行動した";

        public SkillData(IEffectPosition position, IArea area, IEffect effect, int rushDistance, string log)
        {
            Position = position;
            Area = area;
            Effect = effect;
            RushDistance = rushDistance;
            Log = log;
        }

        public string Info()
        {
            var info = $"効果: {Effect.Info()}\n範囲: {Area.Info()}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            return info;
        }
    }
}