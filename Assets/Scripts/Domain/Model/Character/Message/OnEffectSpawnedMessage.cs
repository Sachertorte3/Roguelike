using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Message
{
    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area, Color Color);
}