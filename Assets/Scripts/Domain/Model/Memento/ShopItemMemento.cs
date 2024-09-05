using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopItemMemento
    {
        public string Id;
        public int Price;
    }
}