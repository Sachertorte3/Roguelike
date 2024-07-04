using System.Collections.Generic;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record MapMemento(
        TilemapMemento Tilemap,
        List<CharacterMemento> Characters,
        List<ItemEntityMemento> Items,
        EventEntitiesMemento EventEntities,
        RoomMemento? MonsterHouse,
        ShopMemento? Shop
    );
}