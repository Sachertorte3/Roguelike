using System;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    [Serializable]
    public class ItemEntityMemento
    {
        public ItemMemento Item;
        public EntityMemento Entity;
    }
}