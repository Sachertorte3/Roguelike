using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffectMap
    {
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public IEnumerable<Vector2Int> GetEnemyPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetNeutralPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetAllyPositions(IHasAffiliation character);
    }
}