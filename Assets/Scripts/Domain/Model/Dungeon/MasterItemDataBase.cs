using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<ItemData> GetAllItems()
        {
            var items = new HashSet<ItemData>();
            items.UnionWith(Consumables.Items);
            items.UnionWith(Weapons.Items);
            items.UnionWith(Artifacts.Items);
            items.UnionWith(UpgradeMaterials.Items);
            return items.ToList();
        }
    }
}