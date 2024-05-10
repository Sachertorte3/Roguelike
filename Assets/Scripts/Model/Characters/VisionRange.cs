using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public class VisionRange
    {
        public Observable<HashSet<Vector2Int>> OnVisibleAreaChanged => _visibleAreaCache;
        private ReactiveProperty<HashSet<Vector2Int>> _visibleAreaCache = new ReactiveProperty<HashSet<Vector2Int>>();
        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleAreaCache.Value = Calc(currentPosition));
            Globals.Map.OnChangeTile.Subscribe(_ => _visibleAreaCache.Value = Calc(position.CurrentValue));
        }
        public void Refrash(Vector2Int position)
        {
            _visibleAreaCache.Value = Calc(position);
        }
        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            return ViewCalculator.ComputeCircle(Globals.Map.GetAllPassablePositions(), position, 10f);
        }
        public HashSet<Vector2Int> Get()
        {
            return _visibleAreaCache.CurrentValue;
        }
    }
}
