namespace Domain.Model
{
    public interface IHasPlayerEvent
    {
        public IPlayerEvent Event { get; }
    }
}