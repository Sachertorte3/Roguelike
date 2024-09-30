using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ChestMemento
    {
        [field: SerializeField] public ItemMemento Item { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        public ChestMemento(ItemMemento item, EntityMemento entity)
        {
            Item = item;
            Entity = entity;
        }
    }
}