using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Message
{
    public record OnVisibleAreaChangedMessage(IReadOnlyCollection<Vector2Int> NewArea,
        IReadOnlyCollection<Vector2Int> OldArea);
}