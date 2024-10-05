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
        [field: SerializeField] public List<TrapMemento> Traps { get; private set; }
        [field: SerializeField] public Option<EntityMemento> Bonfire { get; private set; }

        public EventEntitiesMemento(List<StairsMemento> stairs, List<ChestMemento> chests, List<TrapMemento> traps,
            Option<EntityMemento> bonfire)
        {
            Stairs = stairs;
            Chests = chests;
            Traps = traps;
            Bonfire = bonfire;
        }
    }
}