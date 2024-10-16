using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MoneyMemento
    {
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }

        public MoneyMemento(EntityMemento entity, int amount)
        {
            Entity = entity;
            Amount = amount;
        }
    }
}