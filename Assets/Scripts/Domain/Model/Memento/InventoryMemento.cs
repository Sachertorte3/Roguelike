using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StorageMemento
    {
        [field: SerializeField] public List<Option<IItemMemento>> Items { get; private set; }
        [field: SerializeField] public bool CanAddItemsWithStorage { get; private set; }
        [field: SerializeField] public bool CanRemoveItem { get; private set; }

        public StorageMemento(List<Option<IItemMemento>> items, bool canAddItemsWithStorage, bool canRemoveItem)
        {
            Items = items;
            CanAddItemsWithStorage = canAddItemsWithStorage;
            CanRemoveItem = canRemoveItem;
        }
    }
}