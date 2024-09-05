using System;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class InventoryMemento
    {
        public Option<ItemMemento>[] Items;
    }
}