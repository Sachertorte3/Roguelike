using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.Memento;
using R3;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public class ItemDatabase : ITable<ItemData>, ISerializable<ItemDatabaseMemento>
    {
        private MasterItemDataBase _masterItemDataBase;
        private Dictionary<string, string> _plaseholders = new();
        private CategoryPlaceholders _potionPlaceholders;
        private CategoryPlaceholders _scrollPlaceholders;
        private CategoryPlaceholders _bookPlaceholders;
        private CategoryPlaceholders _wandPlaceholders;
        private CategoryPlaceholders _artifactPlaceholders;
        private ItemCategoryWeight _itemCategoryWeight;
        private Subject<Unit> _onItemRenamed = new();
        public Observable<Unit> OnItemRenamed => _onItemRenamed;
        
        public ItemDatabase(ItemDatabaseMemento memento, MasterItemDataBase masterItemDataBase, ItemCategoryWeight itemCategoryWeight)
        {
            _plaseholders = memento.Placeholders;
            _potionPlaceholders = memento.PotionPlaceholders;
            _scrollPlaceholders = memento.ScrollPlaceholders;
            _bookPlaceholders = memento.BookPlaceholders;
            _wandPlaceholders = memento.WandPlaceholders;
            _artifactPlaceholders = memento.ArtifactPlaceholders;
            _masterItemDataBase = masterItemDataBase;
            _itemCategoryWeight = itemCategoryWeight;
        }

        public ItemDatabaseMemento Serialize()
        {
            return new ItemDatabaseMemento(
                _plaseholders,
                _potionPlaceholders,
                _scrollPlaceholders,
                _bookPlaceholders,
                _wandPlaceholders,
                _artifactPlaceholders
            );
        }

        public static ItemDatabaseMemento Build(Placeholders placeholders)
        {
            placeholders.PotionPlaceholders.InitializeCombinedPlaceholders();
            placeholders.ScrollPlaceholders.InitializeCombinedPlaceholders();
            placeholders.BookPlaceholders.InitializeCombinedPlaceholders();
            placeholders.WandPlaceholders.InitializeCombinedPlaceholders();
            placeholders.ArtifactPlaceholders.InitializeCombinedPlaceholders();
            return new ItemDatabaseMemento(
                new Dictionary<string, string>(),
                placeholders.PotionPlaceholders,
                placeholders.ScrollPlaceholders,
                placeholders.BookPlaceholders,
                placeholders.WandPlaceholders,
                placeholders.ArtifactPlaceholders
            );
        }

        public ItemData GetRandomItem() => _masterItemDataBase.GetRandomItem(_itemCategoryWeight.GetRandomCategory());
        public ItemData GetRandomItem(ItemCategory category) => _masterItemDataBase.GetRandomItem(category);

        public string GetPlaceholder(ItemData item) => GetPlaceholder(item.name, item.Category);
        public string GetPlaceholder(string baseName, ItemCategory category)
        {
            if (!_plaseholders.ContainsKey(baseName))
            {
                var placeholder = category switch
                {
                    ItemCategory.Potions => _potionPlaceholders.GetAtRandomAndRemove(),
                    ItemCategory.Scrolls => _scrollPlaceholders.GetAtRandomAndRemove(),
                    ItemCategory.Books => _bookPlaceholders.GetAtRandomAndRemove(),
                    ItemCategory.Wands => _wandPlaceholders.GetAtRandomAndRemove(),
                    ItemCategory.Artifacts => _artifactPlaceholders.GetAtRandomAndRemove(),
                    _ => baseName
                };
                _plaseholders[baseName] = placeholder;
            }
            return _plaseholders[baseName];
        }

        public void Rename(string baseName, string newName)
        {
            if (newName == "")
                return;
            _plaseholders[baseName] = newName;
            _onItemRenamed.OnNext(Unit.Default);
        }
    }
}