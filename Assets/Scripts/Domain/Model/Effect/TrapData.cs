using System;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class TrapData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public ActorlessSkillData Skill { get; private set; }
    }
}