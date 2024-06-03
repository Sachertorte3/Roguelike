using System.Collections.Generic;
using UnityEngine;

namespace Model.Domain.Characters
{
    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area);
}