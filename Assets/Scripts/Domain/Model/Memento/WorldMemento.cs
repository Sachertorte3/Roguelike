namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        public string DungeonDataName;
        public CharacterMemento Player;
        public SerializableDictionary<int, MapMemento> Maps;
        public int ActiveMapId;
    }
}