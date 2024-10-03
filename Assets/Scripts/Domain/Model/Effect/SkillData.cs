using System;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Memento;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public record SkillData : ISkillData
    {
        [field: SerializeReference, Required] public IArea Area { get; private set; }
        [field: SerializeReference, Required] public IEffect Effect { get; private set; }
        [field: SerializeReference, Required] public IEffectPosition Position { get; private set; } = new AtFeet();
        [field: SerializeField] public int RushDistance { get; private set; } = 0;
        [field: SerializeField, Range(0, 1)] public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
        [field: SerializeField, Required] public string Log { get; private set; } = "は行動した";

        public SkillData(IEffectPosition position, IArea area, IEffect effect, int rushDistance, string log)
        {
            Position = position;
            Area = area;
            Effect = effect;
            RushDistance = rushDistance;
            Log = log;
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
            var info = $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            return info;
        }
    }
}