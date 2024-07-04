#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Map
{
    public record TilemapMemento(
        TileData[,] Tiles,
        List<RectInt> Rooms
    );
}