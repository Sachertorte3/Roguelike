#nullable enable
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    internal record BehaviorResult(
        BehaviorState State,
        Vector2Int? TargetPosition
    );
}