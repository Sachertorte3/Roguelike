#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Map
{
    [Serializable]
    public class MapMemento
    {
        public TilemapMemento Tilemap;
        public List<CharacterMemento> Characters;
        public List<ItemEntityMemento> Items;
        public EventEntitiesMemento EventEntities;
        public List<int> KeyCharacters;
        public Option<RoomMemento> MonsterHouse;
        public Option<ShopMemento> Shop;
    }
}
namespace Domain.Model
{
    public class WorldMemento
    {
        public string DungeonDataName;
        public CharacterMemento Player;
        public SerializableDictionary<int, MapMemento> Maps;
        public int ActiveMapId;
    }
}