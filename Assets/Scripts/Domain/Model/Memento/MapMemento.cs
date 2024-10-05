#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MapMemento
    {
        [field: SerializeField] public TilemapMemento Tilemap { get; private set; }
        [field: SerializeField] public List<CharacterMemento> Characters { get; private set; }
        [field: SerializeField] public List<ItemEntityMemento> Items { get; private set; }
        [field: SerializeField] public EventEntitiesMemento EventEntities { get; private set; }
        [field: SerializeField] public List<string> KeyCharacters { get; private set; }
        [field: SerializeField] public Option<RoomMemento> MonsterHouse { get; private set; }
        [field: SerializeField] public Option<ShopMemento> Shop { get; private set; }
        [field: SerializeField] public Vector2Int RandomBlankPosition { get; private set; }

        public MapMemento(TilemapMemento tilemap, List<CharacterMemento> characters, List<ItemEntityMemento> items,
            EventEntitiesMemento eventEntities, List<string> keyCharacters, Option<RoomMemento> monsterHouse,
            Option<ShopMemento> shop, Vector2Int randomBlankPosition)
        {
            Tilemap = tilemap;
            Characters = characters;
            Items = items;
            EventEntities = eventEntities;
            KeyCharacters = keyCharacters;
            MonsterHouse = monsterHouse;
            Shop = shop;
            RandomBlankPosition = randomBlankPosition;
        }
    }
}