using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Domain.Model.Effect
{
    public record UpgradeData(string Description, System.Action Upgrade);
    public interface IHasUpgrades
    {
        public Dictionary<UpgradePath, UpgradeData> GetUpgrades();
        public IEnumerable<UpgradePath> GenerateUpgradePaths() => GetUpgrades().Keys;
        public string GetUpgradeDescription(UpgradePath path)
        {
            var upgrades = GetUpgrades();
            if (!upgrades.Any())
                throw new Exception($"{GetType().Name} does not support upgrades");
            if (!upgrades.ContainsKey(path))
            {
                foreach (var upgrade in upgrades)
                {
                    Debug.Log(upgrade.Key);
                }
                throw new Exception($"UpgradePath {path} not found");
            }
            return upgrades[path].Description;
        }
        public void ApplyUpgrade(UpgradePath path)
        {
            var upgrades = GetUpgrades();
            if (!upgrades.Any())
                throw new Exception($"{GetType().Name} does not support upgrades");
            if (!upgrades.ContainsKey(path))
            {
                foreach (var upgrade in upgrades)
                {
                    Debug.Log(upgrade.Key);
                }
                throw new Exception($"UpgradePath {path} not found");
            }
            upgrades[path].Upgrade();
        }
    }
}