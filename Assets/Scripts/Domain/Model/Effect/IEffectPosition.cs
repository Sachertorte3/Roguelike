using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Effect;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Utilities;

namespace Effect
{
    public interface IEffectPosition : IHasInfo
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);
    }
}