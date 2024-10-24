using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Model.Evaluation;
using Domain.Model.Map;
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
            IMap map)
        {
            var pos = position;
            for (var i = 0; i < CommonSenseParameters.ThrowDistance; i++)
            {
                if (map.At(pos + direction.Vector()).IsBlankIgnoreWall(CanHitLayer.ToArray()))
                {
                    pos += direction.Vector();
                }
                else if (map.At(pos + direction.Vector()).IsPassableOnMap())
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
            IMap map)
        {
            return Get(position, direction, map);
        }

        public float EvaluateHitProbability()
        {
            return 2;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return "着弾地点";
        }
    }
}