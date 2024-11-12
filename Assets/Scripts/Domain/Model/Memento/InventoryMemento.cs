using System;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StorageMemento
    {
        [field: SerializeField] public Option<ItemMemento>[] Items { get; private set; }
        [field: SerializeField] public bool CanAddItemsWithStorage { get; private set; }

        public StorageMemento(Option<ItemMemento>[] items, bool canAddItemsWithStorage)
        {
            Items = items;
            CanAddItemsWithStorage = canAddItemsWithStorage;
        }
    }
}