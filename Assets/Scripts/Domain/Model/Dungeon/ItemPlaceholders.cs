using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.Memento;
using R3;

namespace Domain.Model.Dungeon
{
    public class ItemPlaceholders : ISerializable<ItemPlaceholdersMemento>
    {
        private Placeholders _placeholderData;
        private Dictionary<string, string> _placeholders = new();
        private Dictionary<string, string> _playerAssignedNames = new();
        private PlaceholderIndexes _potionPlaceholderIndexes;
        private PlaceholderIndexes _scrollPlaceholderIndexes;
        private PlaceholderIndexes _bookPlaceholderIndexes;
        private PlaceholderIndexes _wandPlaceholderIndexes;
        private PlaceholderIndexes _artifactPlaceholderIndexes;
        private Subject<Unit> _onItemRenamed = new();
        public Observable<Unit> OnItemRenamed => _onItemRenamed;

        public ItemPlaceholders(ItemPlaceholdersMemento memento, Placeholders placeholders)
        {
            _placeholderData = placeholders;
            _placeholders = memento.Placeholders;
            _playerAssignedNames = memento.PlayerAssignedNames;
            _potionPlaceholderIndexes = memento.PotionPlaceholders;
            _scrollPlaceholderIndexes = memento.ScrollPlaceholders;
            _bookPlaceholderIndexes = memento.BookPlaceholders;
            _wandPlaceholderIndexes = memento.WandPlaceholders;
            _artifactPlaceholderIndexes = memento.ArtifactPlaceholders;
        }

        public ItemPlaceholdersMemento Serialize()
        {
            return new ItemPlaceholdersMemento(
                _placeholders,
                _playerAssignedNames,
                _potionPlaceholderIndexes,
                _scrollPlaceholderIndexes,
                _bookPlaceholderIndexes,
                _wandPlaceholderIndexes,
                _artifactPlaceholderIndexes
            );
        }

        public static ItemPlaceholdersMemento Build(Placeholders placeholders)
        {
            return new ItemPlaceholdersMemento(
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                new PlaceholderIndexes(placeholders.PotionPlaceholders),
                new PlaceholderIndexes(placeholders.ScrollPlaceholders),
                new PlaceholderIndexes(placeholders.BookPlaceholders),
                new PlaceholderIndexes(placeholders.WandPlaceholders),
                new PlaceholderIndexes(placeholders.ArtifactPlaceholders)
            );
        }

        public string GetPlaceholder(ItemData item)
        {
            return GetPlaceholder(item.name, item.Category);
        }

        public string GetPlaceholder(string baseName, ItemCategory category)
        {
            if (_playerAssignedNames.ContainsKey(baseName))
                return _playerAssignedNames[baseName];
            if (!_placeholders.ContainsKey(baseName))
            {
                var placeholder = category switch
                {
                    ItemCategory.Potions => _potionPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .PotionPlaceholders),
                    ItemCategory.Scrolls => _scrollPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .ScrollPlaceholders),
                    ItemCategory.Books => _bookPlaceholderIndexes.GetAtRandomAndRemove(
                        _placeholderData.BookPlaceholders),
                    ItemCategory.Wands => _wandPlaceholderIndexes.GetAtRandomAndRemove(
                        _placeholderData.WandPlaceholders),
                    ItemCategory.Artifacts => _artifactPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .ArtifactPlaceholders),
                    _ => baseName
                };
                _placeholders[baseName] = placeholder;
            }

            return _placeholders[baseName];
        }

        public void Rename(string baseName, string newName)
        {
            if (newName == "")
                return;
            _playerAssignedNames[baseName] = newName;
            _onItemRenamed.OnNext(Unit.Default);
        }

        public void ClearPlayerAssignedNames()
        {
            _playerAssignedNames.Clear();
        }
    }
}