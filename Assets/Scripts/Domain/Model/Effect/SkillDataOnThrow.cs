using System;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class SkillDataOnThrow : IHasInfo
    {
        [SerializeReference][Required] public IArea Area;
        [SerializeReference][Required] public IEffect Effect;
        [Range(0, 1)] public float ProbabilityOfSuccess = CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;

        public SkillDataOnThrow(IArea area, IEffect effect, float probabilityOfSuccess)
        {
            Area = area;
            Effect = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
        }
    }
}