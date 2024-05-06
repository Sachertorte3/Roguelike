using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Model.Characters
{
    public class VisibleArea
    {
        public Observable<HashSet<Vector2Int>> OnVisibleAreaChanged => _visibleAreaCache;
        private ReactiveProperty<HashSet<Vector2Int>> _visibleAreaCache = new ReactiveProperty<HashSet<Vector2Int>>();
        public VisibleArea(ReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleAreaCache.Value = Calc(currentPosition));
            GameManager.World.Map.OnChangeTile.Subscribe(_ => _visibleAreaCache.Value = Calc(position.CurrentValue));
        }
        public void Refrash(Vector2Int position)
        {
            _visibleAreaCache.Value = Calc(position);
        }
        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            return ViewCalculator.ComputeCircle(GameManager.World.Map.GetAllPassablePositions().ToHashSet(), position, 10f);
        }
        public HashSet<Vector2Int> Get()
        {
            return _visibleAreaCache.CurrentValue;
        }
    }
}
