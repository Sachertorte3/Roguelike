using System;
using System.Collections.Generic;
using Assets.Scripts.Utilities;
using R3;
using UnityEngine;

namespace Model.Characters
{
    internal class VisionRange : IDisposable, IVisionRange
    {
        private readonly ReactiveProperty<HashSet<Vector2Int>> _visibleArea = new();
        private readonly SerialDisposable _disposable = new();
        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position)
        {
            position.Subscribe(currentPosition => _visibleArea.Value = Calc(currentPosition));
            Globals.World.ActiveMap.SubscribeToAll(mapLoaded =>
            {
                if (Globals.World.IsLoaded)
                {
                    _disposable.Disposable =
                    mapLoaded.Tilemap.OnChangeTile.Subscribe(_ => _visibleArea.Value = Calc(position.CurrentValue));
                }
            });
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
            if (Globals.World.IsLoaded)
            {
                return ViewCalculator.ComputeCircle(Globals.World.ActiveMap.CurrentValue.Tilemap.GetAllPassablePositions(), position, 10f);
            }
            else
                return new HashSet<Vector2Int>();
        }
    }
    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}