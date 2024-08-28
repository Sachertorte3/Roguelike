using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IHasUpgrades
    {
        public Dictionary<UpgradePath, System.Action> _GetUpgrades();
        public IEnumerable<UpgradePath> GenerateUpgradePaths() => _GetUpgrades().Keys;
        public void ApplyUpgrade(UpgradePath path)
        {
            var upgrades = _GetUpgrades();
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
            upgrades[path]();
        }
    }
}