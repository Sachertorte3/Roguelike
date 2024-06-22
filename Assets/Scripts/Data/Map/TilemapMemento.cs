using System.Collections.Generic;
using Data.Character;

namespace Data.Map
{
    public record TilemapMemento(
        TileData[,] Tiles
    );

    public record MapMemento(
        TilemapMemento Tilemap,
        List<CharacterMemento> Characters,
        List<ItemEntityMemento> Items,
        EventEntitiesMemento EventEntities
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
}