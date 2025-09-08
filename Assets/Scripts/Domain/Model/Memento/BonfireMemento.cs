#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BonfireMemento
    {
        [field: SerializeField] public bool IsFire { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public BonfireMemento(bool isFire, EntityMemento entity)
        {
            IsFire = isFire;
            Entity = entity;
        }
    }
}