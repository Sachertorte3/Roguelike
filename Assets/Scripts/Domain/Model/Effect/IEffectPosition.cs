using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IEffectPosition : IHasInfo, IHasUpgrades
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);
    }
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