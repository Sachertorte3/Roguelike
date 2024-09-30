using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopItemMemento
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        public ShopItemMemento(string id, int price)
        {
            Id = id;
            Price = price;
        }
    }
}