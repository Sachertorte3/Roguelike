using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Domain.Model.Item
{
    public static class HasUpgradesExtensions
    {
        public static void ApplyUpgrade(this IHasUpgrades hasUpgrades, UpgradePath path)
        {
            var name = path.Peek();
            var upgrade = hasUpgrades.GetUpgrades().FirstOrDefault(upgrade => upgrade.UpgradeName == name);
            if (upgrade != null)
            {
                upgrade.Upgrade();
            }
            else
            {
                var child = hasUpgrades.GetChildren()[name];
                child.ApplyUpgrade(path.Pop());
            }
        }

        public static void ApplyDowngrade(this IHasUpgrades hasUpgrades, UpgradePath path)
        {
            var name = path.Peek();
            var downgrade = hasUpgrades.GetUpgrades().FirstOrDefault(upgrade => upgrade.UpgradeName == name);
            if (downgrade != null)
            {
                downgrade.Downgrade();
            }
            else
            {
                var child = hasUpgrades.GetChildren()[name];
                child.ApplyDowngrade(path.Pop());
            }
        }

        public static List<UpgradePath> GetUpgradePathsRecursively(this IHasUpgrades hasUpgrades)
        {
            var paths = new List<UpgradePath>();
            paths.AddRange(hasUpgrades.GetUpgradeNames().Select(name => new UpgradePath(name)));
            foreach (var (childName, child) in hasUpgrades.GetChildren())
            {
                var childrenPaths = child.GetUpgradePathsRecursively().Select(path => path.Prepend(childName));
                paths.AddRange(childrenPaths);
            }

            return paths;
        }
    }
}