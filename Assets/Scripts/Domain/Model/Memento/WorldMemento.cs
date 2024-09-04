using Domain.Model.Map;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        public SerializableDictionary<string, DungeonMemento> Dungeons;
        public CharacterMemento Player;
        public SerializableDictionary<int, MapMemento> Maps;
        public Location CurrentLocation;
    }
}