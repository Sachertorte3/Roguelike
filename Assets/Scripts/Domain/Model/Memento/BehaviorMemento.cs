#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BehaviorMemento
    {
        [field: SerializeField] public BehaviorData Behavior { get; private set; }
        [field: SerializeField] public Option<(Location, Vector2Int)> HomePosition { get; private set; }
        [field: SerializeField] public Option<Vector2Int> LastTargetPosition { get; private set; }

        public BehaviorMemento(
            BehaviorData behavior,
            (Location, Vector2Int)? homePosition,
            Vector2Int? lastTargetPosition
        )
        {
            Behavior = behavior;
            HomePosition = homePosition.ToOption();
            LastTargetPosition = lastTargetPosition.ToOption();
        }
    }
}