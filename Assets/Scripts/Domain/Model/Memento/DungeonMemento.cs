using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        [field: SerializeField] public string DungeonDataName { get; private set; }
        [SerializeField] private SerializableDictionary<int, string> _mapIds;
        public Dictionary<int, string> MapIds => _mapIds.ToDictionary();
        [field: SerializeField] public ItemDatabaseMemento ItemTable { get; private set; }

        public DungeonMemento(string dungeonDataName, Dictionary<int, string> mapIds, ItemDatabaseMemento itemTable)
        {
            DungeonDataName = dungeonDataName;
            _mapIds = mapIds.ToSerializable();
            ItemTable = itemTable;
        }
    }
}