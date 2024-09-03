using System;
using Domain.Model.Character;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemEntityMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}