using System;
using System.Collections.Generic;
using Domain.Model.Character;

namespace Domain.Model.Map
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