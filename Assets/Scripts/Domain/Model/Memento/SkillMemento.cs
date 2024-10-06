#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Memento
{
    public interface ISkillMemento
    {
    }

    [Serializable]
    public class SpawnEffectSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IEffectPosition Position { get; private set; }
        [field: SerializeReference] public IArea Area { get; private set; }
        [field: SerializeReference] public List<IEffect> Effect { get; private set; }
        [field: SerializeField] public int Repeats { get; private set; }
        [field: SerializeField] public int RushDistance { get; private set; }
        [field: SerializeField] public int BackStepDistance { get; private set; }
        [field: SerializeField] public float ProbabilityOfSuccess { get; private set; }
        [field: SerializeField] public string Log { get; private set; }

        public SpawnEffectSkillMemento(IEffectPosition position, IArea area, List<IEffect> effect, int repeats, int rushDistance,
            int backStepDistance, float probabilityOfSuccess, string log)
        {
            Position = position;
            Area = area;
            Effect = effect;
            Repeats = repeats;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ProbabilityOfSuccess = probabilityOfSuccess;
            Log = log;
        }
    }

    [Serializable]
    public class ItemTargetSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IItemEffect ItemEffect { get; private set; }

        public ItemTargetSkillMemento(IItemEffect itemEffect)
        {
            ItemEffect = itemEffect;
        }
    }
}