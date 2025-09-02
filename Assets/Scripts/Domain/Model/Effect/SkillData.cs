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
                info += $"発動回数: {Repeats}回\n";
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