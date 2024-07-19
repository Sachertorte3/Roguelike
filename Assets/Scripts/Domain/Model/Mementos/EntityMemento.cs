using UnityEngine;

namespace Domain.Model.Character
{
    public record EntityMemento(
        int Id,
        Vector2Int Position,
        EntityLayer Layer
    );
}