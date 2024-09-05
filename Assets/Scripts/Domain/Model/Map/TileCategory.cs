namespace Domain.Model.Map
{
    public enum TileCategory
    {
        Floor,
        Wall,
        UnbreakableWall,
        Blank
    }
    public interface IMovementEntity : IEntity
    {
        MovementEntityType Type { get; }
        Location Destination { get; }
    }
}