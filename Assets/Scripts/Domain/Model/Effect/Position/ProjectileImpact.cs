using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

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
            return new[] { map.GetThrowDestination(position, direction, CommonSenseParameters.ThrowDistance, CanHitLayer.ToArray()) };
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

        public string Info()
        {
            return "着弾地点";
        }
    }
}