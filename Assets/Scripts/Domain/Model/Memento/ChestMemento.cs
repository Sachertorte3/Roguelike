using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ChestMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}