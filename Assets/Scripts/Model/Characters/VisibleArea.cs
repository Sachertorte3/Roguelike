using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public class VisibleArea
    {
        private HashSet<Vector2Int> _visibleAreaCache;
        public VisibleArea(ReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleAreaCache = Calc(currentPosition));
            GameManager.World.Map.OnChangeTile.Subscribe(_ => _visibleAreaCache = Calc(position.CurrentValue));
        }
        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            return ViewCalculator.ComputeCircle(GameManager.World.Map.GetAllPassablePositions().ToHashSet(), position, 10f);
        }
        public HashSet<Vector2Int> Get()
        {
            return _visibleAreaCache;
        }
    }
}
