using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Model.Evaluation;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class ProjectileImpact : IActorlessEffectPosition
    {
        [Required] public IconSerializable Icon;
        public List<EntityLayer> CanHitLayer = new() { EntityLayer.Middle };
        public bool IsDirectional => true;
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            var pos = position;
            for (var i = 0; i < CommonSenseParameters.ThrowDistance; i++)
            {
                if (map.IsBlank(pos + direction.Vector(), CanHitLayer.ToArray()))
                {
                    pos += direction.Vector();
                }
                else if (map.IsPassableOnMap(pos + direction.Vector()))
                {
                    pos += direction.Vector();
                    break;
                }
                else
                {
                    break;
                }
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