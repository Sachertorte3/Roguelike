#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Map;
using UnityEngine;

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
        public NullableSerializable<RoomMemento> MonsterHouse;
        public NullableSerializable<ShopMemento> Shop;
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