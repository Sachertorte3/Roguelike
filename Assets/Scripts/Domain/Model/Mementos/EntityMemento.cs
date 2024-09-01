using System;
using UnityEngine;

namespace Domain.Model.Character
{
    [Serializable]
    public class EntityMemento
    {
        public int Id;
        public Vector2Int Position;
        public EntityLayer Layer;
    }
}