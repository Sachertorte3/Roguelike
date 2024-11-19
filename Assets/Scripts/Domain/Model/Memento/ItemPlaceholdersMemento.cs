using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemPlaceholdersMemento
    {
        [field: SerializeField] public SerializableDictionary<string, string> Placeholders { get; private set; }
        [field: SerializeField] public SerializableDictionary<string, string> PlayerAssignedNames { get; private set; }
        [field: SerializeField] public List<int> PotionUsedPlaceholderIndexes { get; private set; }
        [field: SerializeField] public List<int> ScrollUsedPlaceholderIndexes { get; private set; }
        [field: SerializeField] public List<int> BookUsedPlaceholderIndexes { get; private set; }
        [field: SerializeField] public List<int> WandUsedPlaceholderIndexes { get; private set; }
        [field: SerializeField] public List<int> ArtifactUsedPlaceholderIndexes { get; private set; }

        public ItemPlaceholdersMemento(
            Dictionary<string, string> placeholders,
            Dictionary<string, string> playerAssignedNames,
            List<int> potionUsedPlaceholderIndexes,
            List<int> scrollUsedPlaceholderIndexes,
            List<int> bookUsedPlaceholderIndexes,
            List<int> wandUsedPlaceholderIndexes,
            List<int> artifactUsedPlaceholderIndexes
        )
        {
            Placeholders = placeholders.ToSerializable();
            PlayerAssignedNames = playerAssignedNames.ToSerializable();
            PotionUsedPlaceholderIndexes = potionUsedPlaceholderIndexes;
            ScrollUsedPlaceholderIndexes = scrollUsedPlaceholderIndexes;
            BookUsedPlaceholderIndexes = bookUsedPlaceholderIndexes;
            WandUsedPlaceholderIndexes = wandUsedPlaceholderIndexes;
            ArtifactUsedPlaceholderIndexes = artifactUsedPlaceholderIndexes;
        }
    }
}