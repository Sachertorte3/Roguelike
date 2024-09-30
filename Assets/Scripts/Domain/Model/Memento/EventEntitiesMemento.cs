using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EventEntitiesMemento
    {
        [field: SerializeField] public List<StairsMemento> Stairs { get; private set; }
        [field: SerializeField] public List<ChestMemento> Chests { get; private set; }
        [field: SerializeField] public Option<EntityMemento> Bonfire { get; private set; }
        public EventEntitiesMemento(List<StairsMemento> stairs, List<ChestMemento> chests, Option<EntityMemento> bonfire)
        {
            Stairs = stairs;
            Chests = chests;
            Bonfire = bonfire;
        }
    }
}