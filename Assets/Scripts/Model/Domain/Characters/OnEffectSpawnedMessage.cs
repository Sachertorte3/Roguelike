using System.Collections.Generic;
using Data.Effect;
using UnityEngine;

namespace Model.Domain.Characters
{
    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area, Color Color);
}

