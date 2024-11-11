using System;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StorageMemento
    {
        [field: SerializeField] public Option<ItemMemento>[] Items { get; private set; }

        public StorageMemento(Option<ItemMemento>[] items)
        {
            Items = items;
        }
    }
}