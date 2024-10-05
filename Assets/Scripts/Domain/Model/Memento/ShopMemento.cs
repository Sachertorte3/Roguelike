using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopMemento
    {
        [field: SerializeField] public RoomMemento Room { get; private set; }
        [field: SerializeField] public EntityMemento Clerk { get; private set; }
        [field: SerializeField] public List<ShopItemMemento> Items { get; private set; }
        [field: SerializeField] public bool IsStolen { get; private set; }

        public ShopMemento(RoomMemento room, EntityMemento clerk, List<ShopItemMemento> items, bool isStolen)
        {
            Room = room;
            Clerk = clerk;
            Items = items;
            IsStolen = isStolen;
        }
    }
}