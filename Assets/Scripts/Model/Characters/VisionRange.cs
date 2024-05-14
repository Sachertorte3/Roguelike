using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Model.Characters
{
    internal class VisionRange : IDisposable, IVisionRange
    {
        private readonly ReactiveProperty<HashSet<Vector2Int>> _visibleAreaCache = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleAreaCache.Value = Calc(currentPosition));
            Globals.Map.OnChangeTile.Subscribe(_ => _visibleAreaCache.Value = Calc(position.CurrentValue));
        }

        public void Dispose()
        {
            _visibleAreaCache.Dispose();
        }

        public Observable<HashSet<Vector2Int>> OnVisibleAreaChanged => _visibleAreaCache;

        public void Refrash(Vector2Int position)
        {
            _visibleAreaCache.Value = Calc(position);
        }

        public HashSet<Vector2Int> Get()
        {
            return _visibleAreaCache.CurrentValue;
        }

        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            return ViewCalculator.ComputeCircle(Globals.Map.GetAllPassablePositions(), position, 10f);
        }
    }
}