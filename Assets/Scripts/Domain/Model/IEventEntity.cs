namespace Domain.Model
{
    public interface IEventEntity : IHasCharacterEvent, IEntity
    {
    }
    public interface IPlayerEventEntity : IHasPlayerEvent, IEntity
    {
    }
}