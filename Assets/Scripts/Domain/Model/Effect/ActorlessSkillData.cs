using System;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class ActorlessSkillData : IActorlessSkillData
    {
        [field: SerializeReference, Required] public INotDirectionalArea Area { get; private set; }
        [field: SerializeReference, Required] public IActorlessEffect Effect { get; private set; }
        [field: SerializeReference, Required] public IActorlessEffectPosition Position { get; private set; }
        public int RushDistance => 0;
        [field: SerializeField, Range(0, 1)] public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
        public string Log => "";

        public ActorlessSkillData(IActorlessEffectPosition position, INotDirectionalArea area, IActorlessEffect effect, float probabilityOfSuccess)
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