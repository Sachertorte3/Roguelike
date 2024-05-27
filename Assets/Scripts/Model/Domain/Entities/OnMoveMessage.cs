using UnityEngine;
using Utilities;

namespace Model.Domain.Entities
{
    public record OnMoveMessage(Direction8 Direction, Vector2Int Destination);
}