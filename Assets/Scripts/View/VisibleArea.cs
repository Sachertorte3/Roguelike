using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.View
{
    public class VisibleArea
    {
        public Observable<HashSet<Vector2Int>> OnVisibleAreaChanged => _visibleAreaCache;
        private ReactiveProperty<HashSet<Vector2Int>> _visibleAreaCache = new ReactiveProperty<HashSet<Vector2Int>>();
        public VisibleArea()
        {
            _visibleAreaCache.Value = new HashSet<Vector2Int>();
        }
        public void UpdateArea(HashSet<Vector2Int> area)
        {
            _visibleAreaCache.Value = area;
        }
        public HashSet<Vector2Int> Get() => _visibleAreaCache.Value;
    }
}
