using System.Collections.Generic;
using Data;
using Data.Effect;
using UnityEngine;

namespace Effect
{
    public interface IEffectPosition : IHasInfo
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position);
    }
}

