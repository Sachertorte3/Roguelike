using System.Collections.Generic;
using Data.Condition;
using UnityEngine;

namespace Data.Effect
{
    public interface IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public Aggression Aggression { get; }
        public IAffiliation Affiliation { get; }
        public Dictionary<(IConditionData, RemovalConditionData), float> AdditionalConditions { get; }
    }
}