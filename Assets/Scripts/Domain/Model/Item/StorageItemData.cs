#nullable enable

using System.Collections.Generic;
using Domain.Model.Condition;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/StorageItem")]
    public class StorageItemData : ScriptableObject, IItemData
    {
        [Required] public Sprite Icon;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public int AdditionalPrice = 0;
        public float MultiplyPrice = 1f;
        [SerializeReference] public IInventoryEffect? InventoryEffect = null;
        [MinValue(1)] public int StorageCapacity = 0;
        public bool CanRemoveItem = false;
        [MinValue(1), ShowIf("@InventoryEffect != null")] public int UsageLimit;
        public int UpgradeLimit = 3;
        [SerializeReference] public List<IConditionData> PassiveConditions;
    }
}