using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    [Serializable]
    public record SkillData : ISkillData
    {
        [field: SerializeReference]
        [field: Required]
        public IEffectPosition Position { get; private set; } = new AtFeet();

        [field: SerializeReference]
        [field: Required]
        public IArea Area { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public List<IEffect> Effects { get; private set; }

        [field: SerializeField]
        [field: MinValue(1)]
        public int Repeats { get; private set; } = 1;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        [field: SerializeField]
        [field: Required]
        public string Log { get; private set; } = "は行動した";

        public SkillData(IEffectPosition position, IArea area, List<IEffect> effects, int repeats, float probabilityOfSuccess, string log)
        {
            Position = position;
            Area = area;
            Effects = effects;
            Repeats = repeats;
            ProbabilityOfSuccess = probabilityOfSuccess;
            Log = log;
        }

#if UNITY_EDITOR
        public void OnValidate(float probabilityOfSuccess)
        {
            if (Repeats == 0)
            {
                Repeats = 1;
            }

            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = probabilityOfSuccess;
            }
        }
#endif

        public string Info()
        {
            var info = "";
            if (Repeats > 1)
                info += $"効果は{Repeats}回発動する\n";
            info += $"{Position.Info()}の{Area.Info()}を対象に\n";
            foreach (var (effect, index) in Effects.Index())
            {
                info += effect.Info();
            }
            info += $"発動は{ProbabilityOfSuccess:P0}の確率で成功する\n";
            return info;
        }
    }
}