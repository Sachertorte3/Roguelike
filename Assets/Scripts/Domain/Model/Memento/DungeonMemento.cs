using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        public string DungeonDataName;
        public SerializableDictionary<int, int> MapIds;
    }
}