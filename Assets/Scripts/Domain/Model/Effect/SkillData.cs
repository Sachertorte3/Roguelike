using System;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
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
        public int RushDistance = 0;
        [Range(0, 1)] public float ProbabilityOfSuccess = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
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
            var info = $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            return info;
        }
    }
}