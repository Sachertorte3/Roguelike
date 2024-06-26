using System.Collections.Generic;
using Domain.Model.Effect;
using UnityEngine;

namespace Domain.Service.Characters
{
    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area, Color Color);
}