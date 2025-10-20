#nullable enable
using System;
using Domain.Model.Item;
using System.Linq;

namespace Domain.Service.Items
{
    public static class ItemMergeExtension
    {
        public static bool CanSelectForBaseItem(IItem item) => item is DirectWeapon || item.MaxUsages > 0;
        public static bool CanSelectForMergedItem(IItem item, IItem mergeBaseItem)
        {
            switch (mergeBaseItem)
            {
                case DirectWeapon:
                case RangedWeapon:
                    if (item == mergeBaseItem)
                        return false;
                    var featuresToMergeWeapon = item switch
                    {
                        RangedWeapon weapon => weapon.Features,
                        DirectWeapon weapon => weapon.Features,
                        Item mergeditem => mergeditem.FeaturesToMergeWeapon,
                        _ => throw new Exception("Invalid item")
                    };
                    var mergeBaseweaponFeatures = mergeBaseItem switch
                    {
                        RangedWeapon weapon => weapon.Features,
                        DirectWeapon weapon => weapon.Features,
                        _ => throw new Exception("Invalid item")
                    };
                    var featureLimit = mergeBaseItem switch
                    {
                        RangedWeapon weapon => weapon.FeatureLimit,
                        DirectWeapon weapon => weapon.FeatureLimit,
                        _ => throw new Exception("Invalid item")
                    };
                    var applicabilityTag = mergeBaseItem switch
                    {
                        RangedWeapon => FeatureApplicabilityTag.RangedWeapons,
                        DirectWeapon => FeatureApplicabilityTag.DirectWeapons,
                        _ => throw new Exception("Invalid item")
                    };
                    if (!mergeBaseweaponFeatures.Merge(featuresToMergeWeapon, featureLimit, applicabilityTag).SequenceEqual(mergeBaseweaponFeatures))
                    {
                        return true;
                    }
                    if (item.CanUpgrade() && mergeBaseItem.UpgradeCount > 0)
                    {
                        return true;
                    }
                    return false;
                case Item baseItem:
                    return baseItem.BaseName == item.BaseName && baseItem.CanMergeUses;
                default:
                    throw new Exception("Invalid item");
            }
        }
        public static IItem Merge(this IItem mergeBaseItem, IItem mergedItem)
        {
            return mergeBaseItem.Match<IItem>(
                item => item.Merge(mergedItem as Item),
                directWeapon => directWeapon.Merge(mergedItem),
                rangedWeapon => rangedWeapon.Merge(mergedItem)
            );
        }
    }
}