using UnityEngine;

namespace Domain.Model.Character
{
    public record EntityMemento(
        Vector2Int Position,
        EntityLayer Layer
    );
}