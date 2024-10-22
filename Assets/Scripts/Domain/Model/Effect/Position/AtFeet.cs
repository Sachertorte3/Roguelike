using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class AtFeet : IActorlessEffectPosition
    {
        public bool IsDirectional => false;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map) => Get(position, direction, map);
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction,
            IMap map)
        {
            yield return position;
        }

        public float EvaluateHitProbability()
        {
            return 1;
        }


        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return "その場";
        }
    }
}