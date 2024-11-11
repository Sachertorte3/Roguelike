#nullable enable
using System;
using Domain.Model.Character;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BehaviorMemento
    {
        [field: SerializeField] public BehaviorData Behavior { get; private set; }
        [SerializeField] private Option<Location> _homeLocation;
        [SerializeField] private Option<Vector2Int> _homePosition;

        public (Location, Vector2Int)? HomePosition =>
            _homeLocation.HasValue ? (_homeLocation.Value!, _homePosition.Value!) : null;

        [field: SerializeField] public Option<BehaviorState> PreviousState { get; private set; }
        [field: SerializeField] public Option<Vector2Int> PreviousTargetPosition { get; private set; }

        public BehaviorMemento(
            BehaviorData behavior,
            (Location, Vector2Int)? homePosition,
            BehaviorState? previousState,
            Vector2Int? previousTargetPosition
        )
        {
            Behavior = behavior;
            _homeLocation = (homePosition?.Item1).ToOption();
            _homePosition = (homePosition?.Item2).ToOption();
            PreviousState = previousState.ToOption();
            PreviousTargetPosition = previousTargetPosition.ToOption();
        }
    }
}