using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        [field: SerializeField] public string DungeonDataName { get; private set; }
        [SerializeField] private SerializableDictionary<int, string> _mapIds;
        public Dictionary<int, string> MapIds => _mapIds.ToDictionary();

        public DungeonMemento(string dungeonDataName, Dictionary<int, string> mapIds)
        {
            DungeonDataName = dungeonDataName;
            _mapIds = mapIds.ToSerializable();
        }
    }
}