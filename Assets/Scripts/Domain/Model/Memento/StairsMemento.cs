using System;
using Domain.Model.Map;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StairsMemento
    {
        public MovementEntityType Type;
        public Location Destination;
        public string DestinationId;
        public EntityMemento Entity;
    }
}