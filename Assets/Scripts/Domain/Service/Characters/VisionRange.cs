using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Message;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private HashSet<Vector2Int> _visibleArea = new();
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, IMap world)
        {
            position.Subscribe(currentPosition => ChangeVisibleArea(Calc(currentPosition, world)));
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;

        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Refrash(Vector2Int position, IMap world)
        {
            ChangeVisibleArea(Calc(position, world));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            var oldArea = _visibleArea;
            _visibleArea = area;
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, oldArea));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IMap world)
        {
            return ViewCalculator.ComputeCircle(world.GetAllLightPassablePositions(), position, 10f);
        }
    }
}