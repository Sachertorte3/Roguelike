using System;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StairsMemento
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [field: SerializeField] public Location Destination { get; private set; }
        [SerializeField] private string _destinationId;
        public Id<IEntity> DestinationId => new(_destinationId);
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public StairsMemento(MovementEntityType type, Location destination, Id<IEntity> destinationId,
            EntityMemento entity)
        {
            Type = type;
            Destination = destination;
            _destinationId = destinationId.ToString();
            Entity = entity;
        }
    }
}