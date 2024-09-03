#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Memento
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