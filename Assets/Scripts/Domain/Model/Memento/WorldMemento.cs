using System.Collections.Generic;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        public SerializableDictionary<string, DungeonMemento> Dungeons;
        public CharacterMemento Player;
        public SerializableDictionary<string, MapMemento> Maps;
        public Location CurrentLocation;
    }
}