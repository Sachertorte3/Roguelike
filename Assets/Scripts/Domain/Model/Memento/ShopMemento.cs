using System;
using System.Collections.Generic;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopMemento
    {
        public RoomMemento Room;
        public EntityMemento Clerk;
        public List<ShopItemMemento> Items;
        public bool IsStolen;
    }
}