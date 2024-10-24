using System;
using UnityEngine;

namespace Domain.Model.Map
{
    [Serializable]
    public class RoomMemento
    {
        public RectInt Room;
        public bool hasEntered;
        public bool hasEverEntered;
    }
}