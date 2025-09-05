#nullable enable

using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/DirectWeapon")]
    public class DirectWeaponData : ScriptableObject, IItemData
    {
        [Required] public Sprite Icon;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public List<ElementPower> ElementPowers;
        public List<DirectWeaponFeature> Features;
        [MinValue(1)] public int FeatureLimit = 3;
        [MinValue(1)] public int UsageLimit;
        public int UpgradeLimit = 3;
        [SerializeReference] public List<IConditionData> PassiveConditions;
    }
}