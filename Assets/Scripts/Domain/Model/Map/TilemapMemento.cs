#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Map
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
        RoomMemento? MonsterHouse,
        ShopMemento? Shop
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

    public record RoomMemento(
        RectInt Room,
        bool hasEntered,
        bool hasEverEntered
    );
    public record ShopMemento(
        RoomMemento Room,
        List<ItemEntityMemento> Items
    );
}