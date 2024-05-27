using Model.Domain.Entities;
using UnityEngine;

namespace Model.Game
{
    public interface IEventEntity : IHasEvent, IEntity
    {
        public Sprite Icon { get; }
    }
}