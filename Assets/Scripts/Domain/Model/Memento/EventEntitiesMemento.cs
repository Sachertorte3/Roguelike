using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EventEntitiesMemento
    {
        [field: SerializeField] public List<StairsMemento> Stairs { get; private set; }
        [field: SerializeField] public List<ChestMemento> Chests { get; private set; }
        public EventEntitiesMemento(List<StairsMemento> stairs, List<ChestMemento> chests)
        {
            Stairs = stairs;
            Chests = chests;
        }
    }
}