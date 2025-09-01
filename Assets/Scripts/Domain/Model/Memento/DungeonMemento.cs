using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        [SerializeField] private SerializableDictionary<int, string> _mapIds;
        public Dictionary<int, string> MapIds => _mapIds.ToDictionary();

        public DungeonMemento(Dictionary<int, string> mapIds)
        {
            _mapIds = mapIds.ToSerializable();
        }
    }
}