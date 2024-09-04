using System;
using Domain.Model.Character;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DownStairsMemento
    {
        public int DestinationLevel;
        public EntityMemento Entity;
    }
}