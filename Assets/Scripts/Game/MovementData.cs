#nullable enable
using Domain.Model;
using Domain.Model.Map;
using Utilities;

namespace Game
{
    public record MovementData
    {
        public MovementEntityType Type;
        public Location Destination;
        public Id<IEntity>? Id;
        public Id<IEntity>? DestinationId;
        public MovementData(MovementEntityType type, Location destination, Id<IEntity>? id, Id<IEntity>? destinationId)
        {
            Type = type;
            Destination = destination;
            Id = id;
            DestinationId = destinationId;
        }
        public MovementData(MovementEntityType type, Location destination) : this(type, destination, null, null)
        {
        }
    }
}