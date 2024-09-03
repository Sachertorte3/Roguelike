using System;
using Domain.Model.Condition;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ConditionMemento
    {
        [SerializeReference] public IConditionData Condition;
        public RemovalConditionData RemovalCondition;
        public int ElapsedTurns;
    }
}