using System;
using Domain.Model.Effect;
using UnityEngine;

namespace Domain.Model.Entity
{
    [Serializable]
    public class TrapData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public ActorlessSkillData Skill { get; private set; }
        [field: SerializeField, Range(0, 1)] public float ProbabilityOfBreaking { get; private set; } = 0.5f;
    }
}