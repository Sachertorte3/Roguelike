using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class AllCharacter : IActorlessEffectPosition
    {
        public bool IsDirectional => false;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map) => Get(position, direction, map);

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            return map.AllCharacterPositions();
        }

        public float EvaluateHitProbability()
        {
            return 300;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return "すべてのキャラクター";
        }
    }
}