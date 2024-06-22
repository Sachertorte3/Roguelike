using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Data.Effect;
using Model.Domain;
using Model.Domain.Characters;
using UnityEngine;
using Utilities;

namespace Effect.Position
{
    public class AtFeet : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, IEffectMap map)
        {
            yield return position;
        }

        public string Info()
        {
            return "その場";
        }
    }

    public class NearByCharacter : IEffectPosition
    {
        public bool TargetAlly;
        public bool TargetEnemy;
        public bool TargetNeutral;
        public bool TargetSelf;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, IEffectMap map)
        {
            var positions = new List<Vector2Int>();
            if (TargetSelf)
                positions.Add(actor.CurrentPosition);
            if (TargetAlly)
                positions.AddRange(map.GetAllyPositions(actor));
            if (TargetNeutral)
                positions.AddRange(map.GetNeutralPositions(actor));
            if (TargetEnemy)
                positions.AddRange(map.GetEnemyPositions(actor));
            return new[] { positions.MinBy(p => Vector2Int.Distance(p, position)) };
        }

        public string Info()
        {
            return "近くの敵";
        }
    }

    public class ProjectileImpact : IEffectPosition
    {
        [Required] public Sprite Icon;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, IEffectMap map)
        {
            throw new NotImplementedException();
        }

        public string Info()
        {
            return "着弾地点";
        }
    }
}