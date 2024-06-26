using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private readonly ObservableHashSet<Vector2Int> _visibleArea = new();
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, IMap world)
        {
            position.Subscribe(currentPosition => ChangeVisibleArea(Calc(currentPosition, world)));
        }

        public IObservableCollection<Vector2Int> VisibleArea => _visibleArea;

        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Refrash(Vector2Int position, IMap world)
        {
            ChangeVisibleArea(Calc(position, world));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            HashSet<Vector2Int> enterArea = new(area);
            var exitArea = VisibleArea.ToHashSet();
            exitArea.ExceptWith(area);
            enterArea.ExceptWith(VisibleArea);
            _visibleArea.SynchronizeWith(area);
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, exitArea, enterArea));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IMap world)
        {
            return ViewCalculator.ComputeCircle(world.GetAllLightPassablePositions(), position, 10f);
        }
    }
}