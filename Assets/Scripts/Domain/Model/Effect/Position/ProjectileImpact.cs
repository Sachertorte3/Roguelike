using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class ProjectileImpact : IActorlessEffectPosition
    {
        [Required] public IconSerializable Icon;
        public List<EntityLayer> CanHitLayer = new() { EntityLayer.Middle };

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            var pos = position;
            while (map.IsBlank(pos + direction.Vector(), CanHitLayer.ToArray()))
            {
                pos += direction.Vector();
            }

            if (map.IsPassableOnMap(pos + direction.Vector()))
            {
                pos += direction.Vector();
            }

            return new[] { pos };
        }

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            return Get(position, direction, map);
        }

        public float EvaluateHitProbability()
        {
            return 2;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "着弾地点";
        }
    }
}