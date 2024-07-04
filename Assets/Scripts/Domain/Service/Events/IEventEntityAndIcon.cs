using UnityEngine;

namespace Domain.Service.Events
{
    public interface IEventEntityAndIcon : IEventEntity
    {
        public Sprite Icon { get; }
    }
}