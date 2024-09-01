using System;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    [Serializable]
    public class ChestMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}