using System;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class SkillDataOnUse : IHasInfo
    {
        [SerializeReference][Required] public IArea Area;
        [SerializeReference][Required] public IEffect Effect;
        [SerializeReference][Required] public IEffectPosition Position;
        [Range(0, 1)] public float ProbabilityOfSuccess = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        public SkillDataOnUse(IEffectPosition position, IArea area, IEffect effect, float probabilityOfSuccess)
        {
            Position = position;
            Area = area;
            Effect = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
        }
    }
}