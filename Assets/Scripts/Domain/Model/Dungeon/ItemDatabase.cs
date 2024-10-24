using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public class ItemDatabase : ITable<ItemData>
    {
        private MasterItemDataBase _masterItemDataBase;
        private ItemCategoryWeight _itemCategoryWeight;

        public ItemDatabase(MasterItemDataBase masterItemDataBase, ItemCategoryWeight itemCategoryWeight)
        {
            _masterItemDataBase = masterItemDataBase;
            _itemCategoryWeight = itemCategoryWeight;
        }

        public ItemData GetRandomItem() => _masterItemDataBase.GetRandomItem(_itemCategoryWeight.GetRandomCategory());
        public ItemData GetRandomItem(ItemCategory category) => _masterItemDataBase.GetRandomItem(category);
    }
}