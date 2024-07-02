using System;
using Domain.Service.Entities;
using UnityEngine;

namespace Domain.Service.Events
{
    public interface IEventEntity : IHasEvent, IEntity
    {
        public EventTrigger Trigger { get; }
    }

    public interface IEventEntityAndIcon : IEventEntity
    {
        public Sprite Icon { get; }
    }
}