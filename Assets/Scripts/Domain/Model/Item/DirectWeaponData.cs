#nullable enable

using System.Collections.Generic;
using System.Linq;
using Domain.Model.Condition;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/DirectWeapon")]
    public class DirectWeaponData : ScriptableObject, IItemData
    {
        [Required] public Sprite Icon;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        [MinValue(1)] public int Power;
        [SerializeField] private List<StringSerializableDirectWeaponFeature> _features;
        public List<DirectWeaponFeature> Features => _features.Select(feature => feature.Value).ToList();
        [MinValue(1)] public int FeatureLimit = 3;
        [MinValue(1)] public int UsageLimit;
        public int UpgradeLimit = 3;
        [SerializeReference] public List<IConditionData> PassiveConditions;
    }
}