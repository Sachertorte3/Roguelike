using UnityEngine;

namespace Domain.Model.Map
{
    public record RoomMemento(
        RectInt Room,
        bool hasEntered,
        bool hasEverEntered
    );
}