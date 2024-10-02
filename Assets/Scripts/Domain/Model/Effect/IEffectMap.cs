using System.Collections.Generic;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffectMap
    {
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsPassable(Vector2Int position, IAffiliation affiliation);
        public bool IsPassableOnMap(Vector2Int position);
        public IEnumerable<Vector2Int> GetVisibleEnemyPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea);

        public IEnumerable<Vector2Int> GetVisibleNeutralPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea);
        public IEnumerable<Vector2Int> GetVisibleAllyPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea);
    }
}