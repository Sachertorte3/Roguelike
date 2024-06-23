#nullable enable
using System.Collections.Generic;
using Data.Character;
using RandomDungeonWithBluePrint;
using UnityEngine;

namespace Data.Map
{
    public record TilemapMemento(
        TileData[,] Tiles,
        List<RectInt> Rooms
    );

    public record MapMemento(
        TilemapMemento Tilemap,
        List<CharacterMemento> Characters,
        List<ItemEntityMemento> Items,
        EventEntitiesMemento EventEntities,
        MonsterHouseMemento? MonsterHouse
    );

    public record ItemEntityMemento(
        ItemMemento Item,
        EntityMemento Entity
    );

    public record EventEntitiesMemento
    (
        DownStairsMemento DownStairs,
        UpStairsMemento? UpStairs,
        List<ChestMemento> Chests
    );

    public record UpStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    );

    public record DownStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    );

    public record ChestMemento(
        ItemData Item,
        EntityMemento Entity
    );

    public record MonsterHouseMemento(
        RectInt Room,
        bool hasEntered,
        bool hasEverEntered
    );
}