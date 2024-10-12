using System;
using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class MasterItemDataBase
    {
        public RarityWeightTable<ItemData> Potions;
        public RarityWeightTable<ItemData> Scrolls;
        public RarityWeightTable<ItemData> Books;
        public RarityWeightTable<ItemData> Wands;
        public RarityWeightTable<ItemData> Weapons;
        public RarityWeightTable<ItemData> Artifacts;
        public RarityWeightTable<ItemData> Others;
        public RarityWeightTable<ItemData> ChestItems;
        public Table<ShopItemData> ShopItems;

        public ItemData GetRandomItem(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Potions => Potions.GetRandomItem(),
                ItemCategory.Scrolls => Scrolls.GetRandomItem(),
                ItemCategory.Books => Books.GetRandomItem(),
                ItemCategory.Wands => Wands.GetRandomItem(),
                ItemCategory.Weapons => Weapons.GetRandomItem(),
                ItemCategory.Artifacts => Artifacts.GetRandomItem(),
                ItemCategory.Others => Others.GetRandomItem(),
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}