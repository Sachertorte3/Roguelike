using System;
using System.Collections.Generic;
using System.Linq;
using StringSerializableEnum;
using ApplicabilityTag = Domain.Model.Item.FeatureApplicabilityTag;

namespace Domain.Model.Item
{
    [Flags]
    public enum FeatureApplicabilityTag
    {
        None = 0,
        DirectWeapons = 1 << 0,
        RangedWeapons = 1 << 1,
        Weapons = DirectWeapons | RangedWeapons,
        Potions = 1 << 2,
        Scrolls = 1 << 3,
        Wandss = 1 << 4,
        Books = 1 << 5,
        Others = 1 << 6,
        All = ~0
    }
    public enum FeatureExclusionGroup
    {
        None,
        AttackArea,
        AttackPosition,
        AttackElement
    }
    [StringSerializable]
    public enum ItemFeature
    {
        //DirectWeapon AttackArea
        TwoRangeAttack,      // 2マス攻撃
        FanAttack,           // 扇型攻撃
        SpinAttack,          // 回転攻撃

        //RangedWeapon AttackPosition
        ArcingShot,         // 曲射
        Piercing,           // 貫通

        //RangedWeapon AttackArea
        Explosive,          // 爆発

        //Attack Additional
        DoubleAttack,        // 2回攻撃
        Knockback,            // 吹き飛ばし
        Critical,             // クリティカル
        Dig,                  // 掘る
        BreakTrap,            // トラップを破壊
        Absorbing,            // 吸収
        GuaranteedHit,        // 必中
        EnhanceThrow,         // 投擲強化
        Paralysis,             // 麻痺
        Blind,                 // 盲目
        Confusion,             // 混乱
        Sleep,                 // 眠り
        Poison,                // 毒
        Slowness,              // 鈍足
        Restraint,             // 拘束
        EnhanceAbnormalCondition,// 状態異常付与率強化

        //Attack Element
        Fire,
        Ice,
        Thunder,
        Light,
        Dark,

        //Other
        EnhanceDurability,   // 耐久強化
        Artistic,            // 芸術
    }
    public static class DirectWeaponFeatureExtensions
    {
        public static string GetName(this ItemFeature feature)
        {
            return feature switch
            {
                ItemFeature.TwoRangeAttack => "2マス攻撃",
                ItemFeature.FanAttack => "扇型攻撃",
                ItemFeature.SpinAttack => "回転攻撃",

                ItemFeature.ArcingShot => "曲射",
                ItemFeature.Piercing => "貫通",

                ItemFeature.Explosive => "爆発",

                ItemFeature.DoubleAttack => "2回攻撃",
                ItemFeature.Knockback => "吹き飛ばし",
                ItemFeature.Critical => "クリティカル",
                ItemFeature.Dig => "掘る",
                ItemFeature.BreakTrap => "トラップを破壊",
                ItemFeature.Absorbing => "吸収",
                ItemFeature.GuaranteedHit => "必中",
                ItemFeature.EnhanceThrow => "投擲強化",
                ItemFeature.Paralysis => "麻痺",
                ItemFeature.Blind => "盲目",
                ItemFeature.Confusion => "混乱",
                ItemFeature.Sleep => "眠り",
                ItemFeature.Poison => "毒",
                ItemFeature.Slowness => "鈍足",
                ItemFeature.Restraint => "拘束",
                ItemFeature.EnhanceAbnormalCondition => "状態異常付与率強化",

                ItemFeature.EnhanceDurability => "耐久強化",
                ItemFeature.Artistic => "美術品",
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static ApplicabilityTag GetApplicability(this ItemFeature feature)
        {
            return feature switch
            {
                ItemFeature.TwoRangeAttack => ApplicabilityTag.DirectWeapons,
                ItemFeature.FanAttack => ApplicabilityTag.DirectWeapons,
                ItemFeature.SpinAttack => ApplicabilityTag.DirectWeapons,

                ItemFeature.ArcingShot => ApplicabilityTag.RangedWeapons,
                ItemFeature.Piercing => ApplicabilityTag.RangedWeapons,

                ItemFeature.Explosive => ApplicabilityTag.RangedWeapons,

                ItemFeature.DoubleAttack => ApplicabilityTag.Weapons,
                ItemFeature.Knockback => ApplicabilityTag.Weapons,
                ItemFeature.Critical => ApplicabilityTag.Weapons,
                ItemFeature.Dig => ApplicabilityTag.Weapons,
                ItemFeature.BreakTrap => ApplicabilityTag.Weapons,
                ItemFeature.Absorbing => ApplicabilityTag.Weapons,
                ItemFeature.GuaranteedHit => ApplicabilityTag.Weapons,
                ItemFeature.EnhanceThrow => ApplicabilityTag.Weapons,
                ItemFeature.Paralysis => ApplicabilityTag.Weapons,
                ItemFeature.Blind => ApplicabilityTag.Weapons,
                ItemFeature.Confusion => ApplicabilityTag.Weapons,
                ItemFeature.Sleep => ApplicabilityTag.Weapons,
                ItemFeature.Poison => ApplicabilityTag.Weapons,
                ItemFeature.Slowness => ApplicabilityTag.Weapons,
                ItemFeature.Restraint => ApplicabilityTag.Weapons,
                ItemFeature.EnhanceAbnormalCondition => ApplicabilityTag.Weapons,

                ItemFeature.EnhanceDurability => ApplicabilityTag.Weapons,
                ItemFeature.Artistic => ApplicabilityTag.Weapons,
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static FeatureExclusionGroup GetExclusionGroup(this ItemFeature feature)
        {
            return feature switch
            {
                ItemFeature.TwoRangeAttack => FeatureExclusionGroup.AttackArea,
                ItemFeature.FanAttack => FeatureExclusionGroup.AttackArea,
                ItemFeature.SpinAttack => FeatureExclusionGroup.AttackArea,

                ItemFeature.ArcingShot => FeatureExclusionGroup.AttackPosition,
                ItemFeature.Piercing => FeatureExclusionGroup.AttackPosition,

                ItemFeature.Explosive => FeatureExclusionGroup.AttackArea,

                ItemFeature.Fire => FeatureExclusionGroup.AttackElement,
                ItemFeature.Ice => FeatureExclusionGroup.AttackElement,
                ItemFeature.Thunder => FeatureExclusionGroup.AttackElement,
                ItemFeature.Light => FeatureExclusionGroup.AttackElement,
                ItemFeature.Dark => FeatureExclusionGroup.AttackElement,

                _ => FeatureExclusionGroup.None,
            };
        }

        public static bool CanAdd(this IEnumerable<ItemFeature> features, ItemFeature feature, ApplicabilityTag targetApplicability)
        {
            var featureApplicability = feature.GetApplicability();
            if (!featureApplicability.HasFlag(targetApplicability))
            {
                return false;
            }

            var group = feature.GetExclusionGroup();
            if (group != FeatureExclusionGroup.None)
            {
                return !features.Any(f => f.GetExclusionGroup() == group);
            }

            const int CANNOT_OVERLAP = 1;
            var sameCount = features.Count(f => f == feature);
            var maxOverlap = feature switch
            {
                ItemFeature.DoubleAttack => CANNOT_OVERLAP,
                ItemFeature.Knockback => CANNOT_OVERLAP,
                ItemFeature.Critical => 4,
                ItemFeature.Dig => CANNOT_OVERLAP,
                ItemFeature.BreakTrap => CANNOT_OVERLAP,
                ItemFeature.Absorbing => 4,
                ItemFeature.GuaranteedHit => CANNOT_OVERLAP,
                ItemFeature.EnhanceThrow => CANNOT_OVERLAP,
                ItemFeature.Paralysis => CANNOT_OVERLAP,
                ItemFeature.Blind => CANNOT_OVERLAP,
                ItemFeature.Confusion => CANNOT_OVERLAP,
                ItemFeature.Sleep => CANNOT_OVERLAP,
                ItemFeature.Poison => CANNOT_OVERLAP,
                ItemFeature.Slowness => CANNOT_OVERLAP,
                ItemFeature.Restraint => CANNOT_OVERLAP,
                ItemFeature.EnhanceAbnormalCondition => 4,

                ItemFeature.EnhanceDurability => 5,
                ItemFeature.Artistic => CANNOT_OVERLAP,
                _ => throw new Exception($"ItemFeature {feature} should have exclusion group or valid overlap count")
            };
            return sameCount < maxOverlap;
        }
        private static IOrderedEnumerable<ItemFeature> Merge(this IEnumerable<ItemFeature> features, ItemFeature otherFeatures, ApplicabilityTag targetApplicability)
        {
            var allFeatures = features.ToList();

            // 追加可能かチェック
            if (allFeatures.CanAdd(otherFeatures, targetApplicability))
            {
                allFeatures.Add(otherFeatures);
            }

            return allFeatures.OrderBy(f => f);
        }
        public static IOrderedEnumerable<ItemFeature> Merge(this IEnumerable<ItemFeature> features, IEnumerable<ItemFeature> otherFeatures, int maxFeatureCount, ApplicabilityTag targetApplicability)
        {
            var result = features.OrderBy(f => f);
            foreach (var feature in otherFeatures)
            {
                if (result.Count() >= maxFeatureCount)
                    break;
                // 追加可能なものだけ追加
                if (result.CanAdd(feature, targetApplicability))
                {
                    result = result.Merge(feature, targetApplicability);
                }
            }

            return result;
        }
    }
}
