using System.Collections.Generic;
using Data;
using Data.Effect;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Effect
{
    public interface IEffectPosition : IHasInfo
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, IEffectMap map);
    }
    public interface IEffectMap
    {
        public IEnumerable<Vector2Int> GetEnemyPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetNeutralPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetAllyPositions(IHasAffiliation character);
    }
}

