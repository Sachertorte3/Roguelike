using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Effect;
using UnityEngine;
using Utilities;

namespace Effect.Position
{
    public class ProjectileImpact : IEffectPosition
    {
        [Required] public Sprite Icon;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction, IEffectMap map)
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
            return new[] {pos};
        }

        public string Info()
        {
            return "着弾地点";
        }
    }
}