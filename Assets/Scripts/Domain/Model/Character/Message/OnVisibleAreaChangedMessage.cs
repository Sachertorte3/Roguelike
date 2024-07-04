using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Message
{
    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited,
        HashSet<Vector2Int> AreaEntered);
}