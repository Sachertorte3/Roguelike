using System;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        public string DungeonDataName;
        public SerializableDictionary<int, string> MapIds;
    }
}