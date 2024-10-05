using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class RoomMemento
    {
        [field: SerializeField] public RectInt Room { get; private set; }
        [field: SerializeField] public bool HasEntered { get; private set; }
        [field: SerializeField] public bool HasEverEntered { get; private set; }

        public RoomMemento(RectInt room, bool hasEntered, bool hasEverEntered)
        {
            Room = room;
            HasEntered = hasEntered;
            HasEverEntered = hasEverEntered;
        }
    }
}