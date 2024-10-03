using System;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class SkillDataOnThrow : ISkillData
    {
        [field: SerializeReference, Required] public IArea Area { get; private set; }
        [field: SerializeReference, Required] public IEffect Effect { get; private set; }
        public IEffectPosition Position => new AtFeet();
        public int RushDistance => 0;
        [field: SerializeField, Range(0, 1)] public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;
        public string Log => "";

        public SkillDataOnThrow(IArea area, IEffect effect, float probabilityOfSuccess)
        {
            Area = area;
            Effect = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

        public void SetSameEffect(SkillDataOnUse skillDataOnUse)
        {
            Effect = skillDataOnUse.Effect;
        }

#if UNITY_EDITOR
        public void OnValidate(float probabilityOfSuccess)
        {
            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = probabilityOfSuccess;
            }
        }
#endif

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
        }
    }
}