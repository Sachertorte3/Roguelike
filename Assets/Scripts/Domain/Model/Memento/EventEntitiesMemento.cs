using System;
using System.Collections.Generic;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EventEntitiesMemento
    {
        public List<StairsMemento> Stairs;
        public List<ChestMemento> Chests;
    }
}