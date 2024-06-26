using UnityEngine;
using Utilities;

namespace Domain.Service.Entities
{
    public record OnMoveMessage(Direction8 Direction, Vector2Int Destination);
}