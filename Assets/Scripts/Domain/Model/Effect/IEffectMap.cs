using System.Collections.Generic;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffectMap
    {
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsPassable(Vector2Int position);
        public bool IsPassableOnMap(Vector2Int position);
        public IEnumerable<Vector2Int> GetEnemyPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetNeutralPositions(IHasAffiliation character);
        public IEnumerable<Vector2Int> GetAllyPositions(IHasAffiliation character);
    }
}