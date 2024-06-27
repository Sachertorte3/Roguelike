using System;
using Domain.Service.Entities;
using UnityEngine;

namespace Domain.Service.Events
{
    public interface IEventEntity : IDisposable, IHasEvent, IEntity
    {
        public Sprite Icon { get; }
        public EventTrigger Trigger { get; }
    }
}