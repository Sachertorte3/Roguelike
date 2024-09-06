using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemEntityMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}