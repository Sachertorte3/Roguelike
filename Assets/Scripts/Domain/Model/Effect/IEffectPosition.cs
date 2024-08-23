using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IEffectPosition : IHasInfo
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);
        public IEnumerable<UpgradeSkill> GenerateUpgrades();
    }
}