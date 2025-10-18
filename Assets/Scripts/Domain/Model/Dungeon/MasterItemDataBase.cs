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
        public RarityWeightTable<DirectWeaponData> DirectWeapons;
        public RarityWeightTable<IItemData> AllWeapons => Weapons.Concat<IItemData, ItemData, DirectWeaponData>(DirectWeapons);
        public RarityWeightTable<ItemData> Artifacts;
        public RarityWeightTable<ItemData> Others;
        public RarityWeightTable<ItemData> ChestItems;
        public RarityWeightTable<DirectWeaponData> ChestDirectWeapons;
        public RarityWeightTable<IItemData> AllChestItems => ChestItems.Concat<IItemData, ItemData, DirectWeaponData>(ChestDirectWeapons);
        public Table<ShopItemData> ShopItems;

        public IItemData GetRandomItem(ItemCategory category, float progress)
        {
            return category switch
            {
                ItemCategory.Potions => Potions.GetRandomItem(progress),
                ItemCategory.Scrolls => Scrolls.GetRandomItem(progress),
                ItemCategory.Books => Books.GetRandomItem(progress),
                ItemCategory.Wands => Wands.GetRandomItem(progress),
                ItemCategory.Weapons => AllWeapons.GetRandomItem(progress),
                ItemCategory.Artifacts => Artifacts.GetRandomItem(progress),
                ItemCategory.Others => Others.GetRandomItem(progress),
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}