using System;
using System.Collections.Generic;

namespace Domain.Model.Map
{
    [Serializable]
    public class EventEntitiesMemento
    {
        public DownStairsMemento DownStairs;
        public UpStairsMemento UpStairs;
        public List<ChestMemento> Chests;
    }
}