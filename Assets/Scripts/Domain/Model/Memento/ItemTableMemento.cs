using System;
using System.Collections.Generic;
using Domain.Model.Dungeon;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemPlaceholdersMemento
    {
        [field: SerializeField] public SerializableDictionary<string, string> Placeholders { get; private set; }
        [field: SerializeField] public PlaceholderIndexes PotionPlaceholders { get; private set; }
        [field: SerializeField] public PlaceholderIndexes ScrollPlaceholders { get; private set; }
        [field: SerializeField] public PlaceholderIndexes BookPlaceholders { get; private set; }
        [field: SerializeField] public PlaceholderIndexes WandPlaceholders { get; private set; }
        [field: SerializeField] public PlaceholderIndexes ArtifactPlaceholders { get; private set; }

        public ItemPlaceholdersMemento(
            Dictionary<string, string> placeholders,
            PlaceholderIndexes potionPlaceholders,
            PlaceholderIndexes scrollPlaceholders,
            PlaceholderIndexes bookPlaceholders,
            PlaceholderIndexes wandPlaceholders,
            PlaceholderIndexes artifactPlaceholders
        )
        {
            Placeholders = placeholders.ToSerializable();
            PotionPlaceholders = potionPlaceholders;
            ScrollPlaceholders = scrollPlaceholders;
            BookPlaceholders = bookPlaceholders;
            WandPlaceholders = wandPlaceholders;
            ArtifactPlaceholders = artifactPlaceholders;
        }
    }
}