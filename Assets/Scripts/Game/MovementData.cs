using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Utilities;

namespace Game
{
    public record MovementData(MovementEntityType Type, Location Destination, Id<IEntity>? Id,
        Id<IEntity>? DestinationId);
}