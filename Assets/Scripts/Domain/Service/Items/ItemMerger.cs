#nullable enable
using System;
using Domain.Model.Item;
using System.Linq;
using System.Collections.Generic;

namespace Domain.Service.Items
{
    public static class ItemMergeExtension
    {
        public static bool CanSelectForBaseItem(IItem baseItem) => baseItem is DirectWeapon || baseItem.MaxUsages > 0;
        public static bool CanSelectForMergedItem(IItem baseItem, IItem mergeBaseItem)
        {
            switch (mergeBaseItem)
            {
                case DirectWeapon mergeBaseweapon:
                    if (baseItem == mergeBaseweapon)
                        return false;
                    var featuresToMergeWeapon = baseItem switch
                    {
                        DirectWeapon weapon => weapon.Features,
                        StorageItem storageItem => new List<DirectWeaponFeature>(),
                        Item item => item.FeaturesToMergeWeapon,
                        _ => throw new Exception("Invalid item")
                    };
                    var mergeBaseweaponFeatures = mergeBaseweapon.Features;
                    if (!mergeBaseweaponFeatures.Merge(featuresToMergeWeapon).SequenceEqual(mergeBaseweaponFeatures))
                    {
                        return true;
                    }
                    foreach (var upgradePath in baseItem.UpgradePaths)
                    {
                        if (mergeBaseweapon.CanUpgrade(upgradePath.ToString()))
                        {
                            return true;
                        }
                    }
                    return false;
                case StorageItem storageItem:
                    return false;
                case Item item:
                    return item.BaseName == baseItem.BaseName && item.CanMergeUses;
                default:
                    throw new Exception("Invalid item");
            }
        }
        public static IItem Merge(this IItem mergeBaseItem, IItem mergedItem)
        {
            return mergeBaseItem switch
            {
                DirectWeapon weapon => weapon.Merge(mergedItem),
                StorageItem storageItem => throw new Exception("Invalid item"),
                Item item => item.Merge(mergedItem as Item),
                _ => throw new Exception("Invalid item")
            };
        }
    }
}