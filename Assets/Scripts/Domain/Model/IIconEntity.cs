using UnityEngine;

namespace Domain.Model
{
    public interface IIconEntity : IEntity
    {
        public Sprite Icon { get; }
    }
}