using System.Collections.Generic;
using UnityEngine;

namespace Model.Domain.Characters
{
    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited,
        HashSet<Vector2Int> AreaEntered);
}