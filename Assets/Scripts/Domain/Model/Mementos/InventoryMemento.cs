using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class InventoryMemento
    {
        public Option<ItemMemento>[] Items;
    }
}