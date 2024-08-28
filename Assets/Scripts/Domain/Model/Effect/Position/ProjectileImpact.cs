using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class ProjectileImpact : IEffectPosition
    {
        [Required] public IconSerializable Icon;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            var pos = position;
            while (map.IsPassable(pos + direction.Vector()))
            {
                pos += direction.Vector();
            }

            if (map.IsMapPassable(pos + direction.Vector()))
            {
                pos += direction.Vector();
            }

            return new[] { pos };
        }

        public Dictionary<UpgradePath, System.Action> _GetUpgrades() => new();

        public string Info()
        {
            return "着弾地点";
        }
    }
}