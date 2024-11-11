#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class SpawnEffectSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IEffectPosition Position { get; private set; }
        [field: SerializeReference] public IArea Area { get; private set; }
        [field: SerializeReference] public List<IEffect> Effects { get; private set; }
        [field: SerializeField] public int Repeats { get; private set; }
        [field: SerializeField] public int RushDistance { get; private set; }
        [field: SerializeField] public int BackStepDistance { get; private set; }
        [field: SerializeField] public float ProbabilityOfSuccess { get; private set; }
        [field: SerializeField] public string Log { get; private set; }

        public SpawnEffectSkillMemento(IEffectPosition position, IArea area, List<IEffect> effects, int repeats,
            int rushDistance,
            int backStepDistance, float probabilityOfSuccess, string log)
        {
            Position = position;
            Area = area;
            Effects = effects;
            Repeats = repeats;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ProbabilityOfSuccess = probabilityOfSuccess;
            Log = log;
        }
    }
}