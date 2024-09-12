#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MapMemento
    {
        public TilemapMemento Tilemap;
        public List<CharacterMemento> Characters;
        public List<ItemEntityMemento> Items;
        public EventEntitiesMemento EventEntities;
        public List<string> KeyCharacters;
        public Option<RoomMemento> MonsterHouse;
        public Option<ShopMemento> Shop;
        public Vector2Int RandomBlankPosition;
    }
}