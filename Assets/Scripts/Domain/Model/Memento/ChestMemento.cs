using System;
using Domain.Model.Character;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ChestMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}