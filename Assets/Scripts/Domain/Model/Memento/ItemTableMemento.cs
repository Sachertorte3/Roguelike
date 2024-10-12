using System;
using System.Collections.Generic;
using Domain.Model.Dungeon;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemDatabaseMemento
    {
        [field: SerializeField] public SerializableDictionary<string, string> Placeholders { get; private set; }
        [field: SerializeField] public CategoryPlaceholders PotionPlaceholders { get; private set; }
        [field: SerializeField] public CategoryPlaceholders ScrollPlaceholders { get; private set; }
        [field: SerializeField] public CategoryPlaceholders BookPlaceholders { get; private set; }
        [field: SerializeField] public CategoryPlaceholders WandPlaceholders { get; private set; }
        [field: SerializeField] public CategoryPlaceholders ArtifactPlaceholders { get; private set; }

        public ItemDatabaseMemento(
            Dictionary<string, string> placeholders,
            CategoryPlaceholders potionPlaceholders,
            CategoryPlaceholders scrollPlaceholders,
            CategoryPlaceholders bookPlaceholders,
            CategoryPlaceholders wandPlaceholders,
            CategoryPlaceholders artifactPlaceholders
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