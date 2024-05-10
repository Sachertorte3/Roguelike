using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Characters
{
    internal class VisionRange: IVisionRange
    {
        public Observable<VisibleAreaChangedMessage> OnVisibleAreaChanged => _visibleAreaCache.Pairwise().Select(area =>
        {
            HashSet<Vector2Int> newArea = new HashSet<Vector2Int>(area.Current);
            area.Previous.ExceptWith(area.Current);
            area.Current.ExceptWith(area.Previous);
            return new VisibleAreaChangedMessage(newArea, area.Previous, area.Current);
        });
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
    public record VisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}
