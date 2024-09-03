using System;
using Domain.Model.Item;

namespace Domain.Model
{
    [Serializable]
    public class MasterItemDataBase
    {
        public RarityWeightTable<ItemData> Consumables;
        public RarityWeightTable<ItemData> Weapons;
        public RarityWeightTable<ItemData> Artifacts;
        public RarityWeightTable<ItemData> UpgradeMaterials;
        public Table<ShopItemData> ShopItems;
    }
}