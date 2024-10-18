using System.Collections.Generic;
using System.Linq;
using BidirectionalMap;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        [SerializeField] private SerializableDictionary<string, DungeonMemento> _dungeons;
        public Dictionary<string, DungeonMemento> Dungeons => _dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => dungeon.Value);
        [SerializeField] private SerializableDictionary<Location, Location> _magicCircleLocations;
        public BiMap<Location, Location> MagicCircleLocations => new(_magicCircleLocations);
        [field: SerializeField] public CharacterMemento Player { get; private set; }
        [field: SerializeField] public List<string> MapIds { get; private set; }
        [field: SerializeField] public Location CurrentLocation { get; private set; }

        public WorldMemento(Dictionary<string, DungeonMemento> dungeons, BiMap<Location, Location> magicCircleLocations, CharacterMemento player,
            List<string> mapIds, Location currentLocation)
        {
            _dungeons = dungeons.ToSerializable();
            _magicCircleLocations = magicCircleLocations.ToSerializableDictionary(location => location.Key, location => location.Value);
            Player = player;
            MapIds = mapIds;
            CurrentLocation = currentLocation;
        }
    }
}