using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Effect;
using UnityEngine;

namespace Effect.Position
{
    public class AtFeet : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position)
        {
            yield return position;
        }
        public string Info() => "その場";
    }
    public class NearByCharacter : IEffectPosition
    {
        public bool TargetSelf;
        public bool TargetAlly;
        public bool TargetNeutral;
        public bool TargetEnemy;
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position)
        {
            throw new NotImplementedException();
        }
        public string Info() => "近くの敵";
    }
    public class ProjectileImpact : IEffectPosition
    {
        [Required] public Sprite Icon;
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position)
        {
            throw new NotImplementedException();
        }
        public string Info() => "着弾地点";
    }
}