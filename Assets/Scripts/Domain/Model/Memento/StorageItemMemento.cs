#nullable enable
using System;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StorageItemMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [field: SerializeField] public Option<InventoryTargetSkillMemento> SkillOnUse { get; private set; }
        [field: SerializeField] public StorageMemento Storage { get; private set; }

        public StorageItemMemento(
            BaseItemMemento baseItem,
            Option<InventoryTargetSkillMemento> skillOnUse,
            StorageMemento storage
        )
        {
            BaseItem = baseItem;
            SkillOnUse = skillOnUse;
            Storage = storage;
        }

        public StorageItemMemento CopyWith(
            BaseItemMemento? baseItem = null,
            Option<InventoryTargetSkillMemento>? skillOnUse = null,
            StorageMemento? storage = null
        )
        {
            return new StorageItemMemento(
                baseItem ?? BaseItem,
                skillOnUse ?? SkillOnUse,
                storage ?? Storage
            );
        }
    }
}