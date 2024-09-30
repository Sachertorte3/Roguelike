using System;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class InventoryMemento
    {
        [field: SerializeField] public Option<ItemMemento>[] Items { get; private set; }
        public InventoryMemento(Option<ItemMemento>[] items)
        {
            Items = items;
        }
    }
}