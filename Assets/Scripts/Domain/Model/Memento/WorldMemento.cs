using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        [SerializeField] private SerializableDictionary<string, DungeonMemento> _dungeons;
        public Dictionary<string, DungeonMemento> Dungeons => _dungeons;
        [field: SerializeField] public CharacterMemento Player { get; private set; }
        [SerializeField] private SerializableDictionary<string, MapMemento> _maps;
        public Dictionary<string, MapMemento> Maps => _maps;
        [field: SerializeField] public Location CurrentLocation { get; private set; }
        public WorldMemento(Dictionary<string, DungeonMemento> dungeons, CharacterMemento player, Dictionary<string, MapMemento> maps, Location currentLocation)
        {
            _dungeons = dungeons.ToSerializable();
            Player = player;
            _maps = maps.ToSerializable();
            CurrentLocation = currentLocation;
        }
    }
}