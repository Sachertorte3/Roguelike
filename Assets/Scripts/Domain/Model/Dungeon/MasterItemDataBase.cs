using System;
using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
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