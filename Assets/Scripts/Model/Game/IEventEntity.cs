using System;
using Data.Map;
using Model.Domain.Entities;
using UnityEngine;

namespace Model.Game
{
    public interface IEventEntity : IDisposable, IHasEvent, IEntity
    {
        public Sprite Icon { get; }
    }
}