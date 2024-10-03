using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IActorlessEffectPosition : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IEffectMap map);
    }
}