using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class AtFeet : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            yield return position;
        }

        public IEnumerable<UpgradeSkill> GenerateUpgrades()
        {
            return new List<UpgradeSkill>();
        }

        public string Info()
        {
            return "その場";
        }
    }
}