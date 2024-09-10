using System;
using Domain.Model.Item;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class ShopItemData
    {
        public RarityWeightTable<ItemData> Items;
    }
}