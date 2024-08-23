#nullable enable
using System;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface ISkillMemento { }
    [Serializable]
    public class SpawnEffectSkillMemento : ISkillMemento
    {
        [SerializeReference] public IEffectPosition Position;
        [SerializeReference] public IArea Area;
        [SerializeReference] public IEffect Effect;
        public int RushDistance;
        public string Log;
    }
    [Serializable]
    public class ItemTargetSkillMemento : ISkillMemento
    {
        [SerializeReference] public IItemEffect ItemEffect;
    }
}