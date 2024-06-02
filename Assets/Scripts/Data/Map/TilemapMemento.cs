using System;
using System.Collections.Generic;
using Data.Character;
using UnityEngine;

namespace Data.Map
{
    public record TilemapMemento(
        TileData[,] Tiles
    );
    public record MapMemento(
        TilemapMemento Tilemap,
        List<CharacterMemento> Players,
        List<ItemEntityMemento> Items,
        DownStairsMemento DownStairs,
        UpStairsMemento? UpStairs
    );
    public record ItemEntityMemento(
        ItemMemento Item,
        EntityMemento Entity
    );
    public interface IEventEntityMemento {}
    public record UpStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    ) : IEventEntityMemento;
    public record DownStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    ) : IEventEntityMemento;
}