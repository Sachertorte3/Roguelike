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
        [SerializeField] private Option<Location> _homeLocation;
        [SerializeField] private Option<Vector2Int> _homePosition;
        public (Location, Vector2Int)? HomePosition => _homeLocation.HasValue ? (_homeLocation.Value!, _homePosition.Value!) : null;
        [field: SerializeField] public Option<Vector2Int> LastTargetPosition { get; private set; }

        public BehaviorMemento(
            BehaviorData behavior,
            (Location, Vector2Int)? homePosition,
            Vector2Int? lastTargetPosition
        )
        {
            Behavior = behavior;
            _homeLocation = (homePosition?.Item1).ToOption();
            _homePosition = (homePosition?.Item2).ToOption();
            LastTargetPosition = lastTargetPosition.ToOption();
        }
    }
}