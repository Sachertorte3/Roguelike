using System;
using Domain.Model.Character;

namespace Domain.Model.Memento
{
    [Serializable]
    public class UpStairsMemento
    {
        public int DestinationMapId;
        public EntityMemento Entity;
    }
}