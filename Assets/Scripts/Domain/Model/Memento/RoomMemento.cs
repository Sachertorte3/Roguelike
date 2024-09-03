using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class RoomMemento
    {
        public RectInt Room;
        public bool hasEntered;
        public bool hasEverEntered;
    }
}