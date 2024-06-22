using System;
using Model.Domain.Entities;
using UnityEngine;

namespace Model.Domain.Events
{
    public interface IEventEntity : IDisposable, IHasEvent, IEntity
    {
        public Sprite Icon { get; }
        public EventTrigger Trigger { get; }
    }

    public enum EventTrigger
    {
        Tread,
        Touch
    }
}