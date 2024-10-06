using System;
using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public class ItemTable : ITable<ItemData>
    {
        private readonly MasterItemDataBase _masterItemDataBase;
        private readonly ItemCategoryWeight _itemCategoryWeight;
        public ItemTable(MasterItemDataBase masterItemDataBase, ItemCategoryWeight itemCategoryWeight)
        {
            _masterItemDataBase = masterItemDataBase;
            _itemCategoryWeight = itemCategoryWeight;
        }

        public ItemData GetRandomItem()
        {
            return _itemCategoryWeight.GetRandomCategory() switch
            {
                ItemCategory.Consumables => _masterItemDataBase.Consumables.GetRandomItem(),
                ItemCategory.Weapons => _masterItemDataBase.Weapons.GetRandomItem(),
                ItemCategory.Artifacts => _masterItemDataBase.Artifacts.GetRandomItem(),
                ItemCategory.UpgradeMaterials => _masterItemDataBase.UpgradeMaterials.GetRandomItem(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public ItemData GetRandomItem(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Consumables => _masterItemDataBase.Consumables.GetRandomItem(),
                ItemCategory.Weapons => _masterItemDataBase.Weapons.GetRandomItem(),
                ItemCategory.Artifacts => _masterItemDataBase.Artifacts.GetRandomItem(),
                ItemCategory.UpgradeMaterials => _masterItemDataBase.UpgradeMaterials.GetRandomItem(),
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}