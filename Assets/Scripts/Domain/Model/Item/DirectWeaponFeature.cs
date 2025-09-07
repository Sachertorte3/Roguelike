using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Model.Item
{
    public enum DirectWeaponFeature
    {
        //MEMO: Due to the merge specifications, it should not affect anything other than Skill.
        TwoRangeAttack,      // 2マス攻撃
        FanAttack,           // 扇型攻撃
        SpinAttack,          // 回転攻撃
        DoubleAttack,        // 2回攻撃
        Knockback,            // 吹き飛ばし
        Critical,             // クリティカル
        Dig,                  // 掘る
        Absorbing,            // 吸収
        GuaranteedHit,        // 必中
        ThrowEnhance,         // 投擲強化
        Paralysis,             // 麻痺
        Blind,                 // 盲目
        Confusion,             // 混乱
        Sleep,                 // 眠り
        Poison,                // 毒
        Slowness,              // 鈍足
        Restraint,             // 拘束
        AbnormalConditionEnhance,    // 状態異常付与率強化
        Artistic,                  // 芸術
    }
    public static class DirectWeaponFeatureExtensions
    {
        public static string GetName(this DirectWeaponFeature feature)
        {
            return feature switch
            {
                DirectWeaponFeature.TwoRangeAttack => "2マス攻撃",
                DirectWeaponFeature.FanAttack => "扇型攻撃",
                DirectWeaponFeature.SpinAttack => "回転攻撃",
                DirectWeaponFeature.DoubleAttack => "2回攻撃",
                DirectWeaponFeature.Knockback => "吹き飛ばし",
                DirectWeaponFeature.Critical => "クリティカル",
                DirectWeaponFeature.Dig => "掘る",
                DirectWeaponFeature.Absorbing => "吸収",
                DirectWeaponFeature.GuaranteedHit => "必中",
                DirectWeaponFeature.ThrowEnhance => "投擲強化",
                DirectWeaponFeature.Paralysis => "麻痺",
                DirectWeaponFeature.Blind => "盲目",
                DirectWeaponFeature.Confusion => "混乱",
                DirectWeaponFeature.Sleep => "眠り",
                DirectWeaponFeature.Poison => "毒",
                DirectWeaponFeature.Slowness => "鈍足",
                DirectWeaponFeature.Restraint => "拘束",
                DirectWeaponFeature.AbnormalConditionEnhance => "状態異常付与率強化",
                DirectWeaponFeature.Artistic => "美術品",
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static int CanOverlap(this DirectWeaponFeature feature)
        {
            const int CANNOT_OVERLAP = 1;
            return feature switch
            {
                DirectWeaponFeature.TwoRangeAttack => CANNOT_OVERLAP,
                DirectWeaponFeature.FanAttack => CANNOT_OVERLAP,
                DirectWeaponFeature.SpinAttack => CANNOT_OVERLAP,
                DirectWeaponFeature.DoubleAttack => CANNOT_OVERLAP,
                DirectWeaponFeature.Knockback => CANNOT_OVERLAP,
                DirectWeaponFeature.Critical => 4,
                DirectWeaponFeature.Dig => CANNOT_OVERLAP,
                DirectWeaponFeature.Absorbing => 4,
                DirectWeaponFeature.GuaranteedHit => CANNOT_OVERLAP,
                DirectWeaponFeature.ThrowEnhance => CANNOT_OVERLAP,
                DirectWeaponFeature.Paralysis => CANNOT_OVERLAP,
                DirectWeaponFeature.Blind => CANNOT_OVERLAP,
                DirectWeaponFeature.Confusion => CANNOT_OVERLAP,
                DirectWeaponFeature.Sleep => CANNOT_OVERLAP,
                DirectWeaponFeature.Poison => CANNOT_OVERLAP,
                DirectWeaponFeature.Slowness => CANNOT_OVERLAP,
                DirectWeaponFeature.Restraint => CANNOT_OVERLAP,
                DirectWeaponFeature.AbnormalConditionEnhance => CANNOT_OVERLAP,
                DirectWeaponFeature.Artistic => CANNOT_OVERLAP,
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static IOrderedEnumerable<DirectWeaponFeature> Merge(this IEnumerable<DirectWeaponFeature> features, DirectWeaponFeature otherFeatures)
        {
            var allFeatures = features.ToList();
            
            allFeatures.Add(otherFeatures);

            var groupedFeatures = allFeatures
                .GroupBy(f => f)
                .SelectMany(g => Enumerable.Repeat(g.Key, Math.Min(g.Count(), g.Key.CanOverlap())))
                .OrderBy(f => f);

            return groupedFeatures;
        }
        public static IOrderedEnumerable<DirectWeaponFeature> Merge(this IEnumerable<DirectWeaponFeature> features, IEnumerable<DirectWeaponFeature> otherFeatures)
        {
            var allFeatures = features.Concat(otherFeatures);

            var groupedFeatures = allFeatures
                .GroupBy(f => f)
                .SelectMany(g => Enumerable.Repeat(g.Key, Math.Min(g.Count(), g.Key.CanOverlap())))
                .OrderBy(f => f);

            return groupedFeatures;
        }
    }
}
