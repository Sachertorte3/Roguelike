using Domain.Model;

namespace Domain.Service.Events
{
    public interface IEventEntity : IHasEvent, IEntity
    {
        public EventTrigger Trigger { get; }
    }
}