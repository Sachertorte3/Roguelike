using UnityEngine;

namespace Domain.Model.Entity
{
    public interface IIconEntity : IEntity
    {
        public Sprite Icon { get; }
    }
}