#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MapMemento
    {
        [SerializeField] private string _id;
        public Id<IMap> Id => new(_id);
        [field: SerializeField] public Location Location { get; private set; }
        [field: SerializeField] public TilemapMemento Tilemap { get; private set; }
        [field: SerializeField] public List<CharacterMemento> Characters { get; private set; }
        [field: SerializeField] public List<ItemEntityMemento> Items { get; private set; }
        [field: SerializeField] public EventEntitiesMemento EventEntities { get; private set; }
        [field: SerializeField] public FireEntitiesMemento Fires { get; private set; }
        [field: SerializeField] public List<string> KeyCharacters { get; private set; }
        [field: SerializeField] public Option<RoomMemento> MonsterHouse { get; private set; }
        [field: SerializeField] public Option<ShopMemento> Shop { get; private set; }
        [field: SerializeField] public Vector2Int RandomBlankPosition { get; private set; }

        public MapMemento(Id<IMap> id, Location location, TilemapMemento tilemap, List<CharacterMemento> characters,
            List<ItemEntityMemento> items, EventEntitiesMemento eventEntities, FireEntitiesMemento fireEntities,
            List<string> keyCharacters,
            Option<RoomMemento> monsterHouse, Option<ShopMemento> shop, Vector2Int randomBlankPosition)
        {
            _id = id.ToString();
            Location = location;
            Tilemap = tilemap;
            Characters = characters;
            Items = items;
            EventEntities = eventEntities;
            Fires = fireEntities;
            KeyCharacters = keyCharacters;
            MonsterHouse = monsterHouse;
            Shop = shop;
            RandomBlankPosition = randomBlankPosition;
        }
    }
}