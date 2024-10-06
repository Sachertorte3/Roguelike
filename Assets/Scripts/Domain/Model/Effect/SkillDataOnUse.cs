using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

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
        public List<IEffect> Effects { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public IEffectPosition Position { get; private set; }

        public int RushDistance => 0;

        public int BackStepDistance => 0;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        public string Log => "";

        public SkillDataOnUse(IEffectPosition position, IArea area, List<IEffect> effects, float probabilityOfSuccess)
        {
            Position = position;
            Area = area;
            Effects = effects;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
            }
        }
#endif

        public string Info()
        {
            var info = "";
            foreach (var (effect, index) in Effects.Index())
            {
                info += $"効果{index + 1}: {effect.Info()}\n";
            }
            info += $"発動位置: {Position.Info()}\n";
            info += $"範囲: {Area.Info()}\n";
            info += $"発動確率: {ProbabilityOfSuccess:P0}";
            return info;
        }
    }
}