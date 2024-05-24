using Model.Domain;
using ObservableCollections;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Model.Characters
{
    internal class VisionRange : IVisionRange
    {
        public IObservableCollection<Vector2Int> VisibleArea => _visibleArea;
        private readonly ObservableHashSet<Vector2Int> _visibleArea = new();
        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, IWorld world)
        {
            position.Subscribe(currentPosition => ChangeVisibleArea(Calc(currentPosition, world)));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            HashSet<Vector2Int> enterArea = new(area);
            HashSet<Vector2Int> exitArea = VisibleArea.ToHashSet();
            exitArea.ExceptWith(area);
            enterArea.ExceptWith(VisibleArea);
            _visibleArea.SynchronizeWith(area);
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, exitArea, enterArea));
        }

        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public void Refrash(Vector2Int position, IWorld world)
        {
            ChangeVisibleArea(Calc(position, world));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IWorld world)
        {
            if (world.IsLoaded)
            {
                return ViewCalculator.ComputeCircle(world.GetAllLightPassablePositions(), position, 10f);
            }
            else
            {
                return new();
            }
        }
    }
}