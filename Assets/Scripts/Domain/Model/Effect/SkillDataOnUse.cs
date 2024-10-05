using System;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class SkillDataOnUse : ISkillData
    {
        [field: SerializeReference]
        [field: Required]
        public IArea Area { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public IEffect Effect { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public IEffectPosition Position { get; private set; }

        public int RushDistance => 0;

        public int BackStepDistance => 0;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        public string Log => "";

        public SkillDataOnUse(IEffectPosition position, IArea area, IEffect effect, float probabilityOfSuccess)
        {
            Position = position;
            Area = area;
            Effect = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
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
            return $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
        }
    }
}