namespace Domain.Model.Map
{
    public interface IMovementEntity : IEntity
    {
        MovementEntityType Type { get; }
        Location Destination { get; }
    }
}