using System;
using UnityEngine;

namespace Domain.Model.Map
{
    [Serializable]
    public class MapConnection
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [field: SerializeField] public Location Destination { get; private set; }

        public MapConnection(MovementEntityType type, Location destination)
        {
            Type = type;
            Destination = destination;
        }
    }
}