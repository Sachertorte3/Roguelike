using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EntityMemento
    {
        public int Id;
        public Vector2Int Position;
        public EntityLayer Layer;
    }
}