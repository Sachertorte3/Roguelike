using Utilities;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Characters
{
    internal class VisionRange : IDisposable, IVisionRange
    {
        private readonly ReactiveProperty<HashSet<Vector2Int>> _visibleArea = new(new());
        private readonly SerialDisposable _disposable = new();
        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleArea.Value = Calc(currentPosition));
        }

        public void Dispose()
        {
            _visibleArea.Dispose();
        }

        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _visibleArea.Pairwise().Select(visibleAreaChanged =>
        {
            HashSet<Vector2Int> newArea = new(visibleAreaChanged.Current);
            visibleAreaChanged.Previous.ExceptWith(visibleAreaChanged.Current);
            visibleAreaChanged.Current.ExceptWith(visibleAreaChanged.Previous);
            return new OnVisibleAreaChangedMessage(newArea, visibleAreaChanged.Previous, visibleAreaChanged.Current);
        });

        public void Refrash(Vector2Int position)
        {
            _visibleArea.Value = Calc(position);
        }

        public HashSet<Vector2Int> Get()//FIX: Changing the get value seems to affect the original value
        {
            return _visibleArea.CurrentValue;
        }

        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            return ViewCalculator.ComputeCircle(Globals.World.ActiveMap.CurrentValue.Tilemap.GetAllPassablePositions(), position, 10f);
        }
    }
    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}