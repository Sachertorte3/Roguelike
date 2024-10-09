using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopMemento
    {
        [field: SerializeField] public RoomMemento Room { get; private set; }
        [SerializeField] private string _clerkId;
        public Id<IEntity> ClerkId => new Id<IEntity>(_clerkId);
        [field: SerializeField] public List<ShopItemMemento> Items { get; private set; }
        [field: SerializeField] public bool IsStolen { get; private set; }

        public ShopMemento(RoomMemento room, Id<IEntity> clerkId, List<ShopItemMemento> items, bool isStolen)
        {
            Room = room;
            _clerkId = clerkId.ToString();
            Items = items;
            IsStolen = isStolen;
        }
    }
}