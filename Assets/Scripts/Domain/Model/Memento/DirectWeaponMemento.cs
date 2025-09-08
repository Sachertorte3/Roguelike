#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DirectWeaponMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [field: SerializeField] public Option<WeaponPrefix> Prefix { get; private set; }
        [field: SerializeField] public List<ElementPower> ElementPowers { get; private set; }
        [field: SerializeField] public List<DirectWeaponFeature> Features { get; private set; }
        [field: SerializeField] public int FeatureLimit { get; private set; }
        [field: SerializeField] public SpawnEffectSkillMemento SkillOnUse { get; private set; }
        [field: SerializeField] public SpawnEffectSkillMemento SkillOnThrow { get; private set; }

        public DirectWeaponMemento(
            BaseItemMemento baseItem,
            Option<WeaponPrefix> prefix,
            List<ElementPower> elementPowers,
            List<DirectWeaponFeature> features,
            int featureLimit,
            SpawnEffectSkillMemento skillOnUse,
            SpawnEffectSkillMemento skillOnThrow
        )
        {
            BaseItem = baseItem;
            Prefix = prefix;
            ElementPowers = elementPowers;
            Features = features;
            FeatureLimit = featureLimit;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
        }

        public DirectWeaponMemento CopyWith(
            BaseItemMemento? baseItem = null,
            Option<WeaponPrefix>? prefix = null,
            List<ElementPower>? elementPowers = null,
            List<DirectWeaponFeature>? features = null,
            int? featureLimit = null,
            SpawnEffectSkillMemento? skillOnUse = null,
            SpawnEffectSkillMemento? skillOnThrow = null
        )
        {
            return new DirectWeaponMemento(
                baseItem ?? BaseItem,
                prefix ?? Prefix,
                elementPowers ?? ElementPowers,
                features ?? Features,
                featureLimit ?? FeatureLimit,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow
            );
        }
    }
}