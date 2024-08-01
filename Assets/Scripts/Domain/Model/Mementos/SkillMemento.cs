#nullable enable
using System;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using UnityEngine;

namespace Domain.Model.Character
{
    [Serializable]
    public class SkillMemento
    {
        [SerializeReference] public IEffectPosition Position;
        [SerializeReference] public IArea Area;
        [SerializeReference] public IEffect Effect;
        public int RushDistance;
        public string? Log;
    }
}