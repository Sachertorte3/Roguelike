using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public class ItemDatabase : ICorrectionTable<ItemData>
    {
        private MasterItemDataBase _masterItemDataBase;
        private ItemCategoryWeight _itemCategoryWeight;

        public ItemDatabase(MasterItemDataBase masterItemDataBase, ItemCategoryWeight itemCategoryWeight)
        {
            _masterItemDataBase = masterItemDataBase;
            _itemCategoryWeight = itemCategoryWeight;
        }

        public ItemData GetRandomItem(float progress)
        {
            return _masterItemDataBase.GetRandomItem(_itemCategoryWeight.GetRandomCategory(), progress);
        }

        public ItemData GetRandomItem(ItemCategory category, float progress)
        {
            return _masterItemDataBase.GetRandomItem(category, progress);
        }
    }
}