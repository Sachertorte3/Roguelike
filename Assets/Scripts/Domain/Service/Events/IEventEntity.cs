using System;
using Domain.Model;
using Domain.Service.Entities;
using R3;

namespace Domain.Service.Events
{
    public interface IEventEntity : IHasEvent, IEntity
    {
        public EventTrigger Trigger { get; }
    }
}