using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class InventoryMemento
    {
        public NullableSerializable<ItemMemento>[] Items;
    }
}