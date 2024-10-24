using System;
using Domain.Model.Condition;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ConditionMemento
    {
        [field: SerializeReference] public IConditionData Condition { get; private set; }
        [field: SerializeField] public RemovalConditionData RemovalCondition { get; private set; }
        [field: SerializeField] public int ElapsedTurns { get; private set; }

        public ConditionMemento(IConditionData condition, RemovalConditionData removalCondition, int elapsedTurns)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
            ElapsedTurns = elapsedTurns;
        }
    }
}