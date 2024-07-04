using System;
using Domain.Service.Entities;

namespace Domain.Service.Events
{
    public interface IEventEntity : IHasEvent, IEntity
    {
        public EventTrigger Trigger { get; }
    }
}