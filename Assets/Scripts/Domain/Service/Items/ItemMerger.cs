#nullable enable
using System;
using Domain.Model.Item;
using System.Linq;

namespace Domain.Service.Items
{
    public static class ItemMergeExtension
    {
        public static bool CanSelectForBaseItem(IItem baseItem) => baseItem is DirectWeapon;
        public static bool CanSelectForMergedItem(BaseItem baseItem, DirectWeapon mergeBaseItem)
        {
            if (baseItem == mergeBaseItem)
                return false;
            var featuresToMergeWeapon = baseItem switch
            {
                DirectWeapon weapon => weapon.Features,
                Item item => item.FeaturesToMergeWeapon,
                _ => throw new Exception("Invalid item")
            };
            var mergeBaseItemFeatures = mergeBaseItem.Features;
            if (!mergeBaseItemFeatures.Merge(featuresToMergeWeapon).SequenceEqual(mergeBaseItemFeatures))
            {
                return true;
            }
            foreach (var upgradePath in baseItem.UpgradePaths)
            {
                if (mergeBaseItem.CanUpgrade(upgradePath.ToString()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}