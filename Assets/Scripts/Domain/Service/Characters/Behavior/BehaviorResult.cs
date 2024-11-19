#nullable enable
using System;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    internal record BehaviorResult(
        BehaviorState State,
        Vector2Int? TargetPosition
    )
    {
        public bool IsDiscoveringCharacter()
        {
            return State switch
            {
                BehaviorState.None => false,
                BehaviorState.DiscoveringEnemy => true,
                BehaviorState.DiscoveringLeader => true,
                BehaviorState.ApproachingToObserve => false,
                BehaviorState.ReturningHome => false,
                BehaviorState.MovingToLastKnownEnemyPosition => false,
                BehaviorState.MovingToLastKnownLeaderPosition => false,
                BehaviorState.Wandering => false,
                _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
            };
        }
    }
}