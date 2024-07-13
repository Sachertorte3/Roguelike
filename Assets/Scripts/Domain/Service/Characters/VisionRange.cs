using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Message;
using R3;
using UnityEngine;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private ReadOnlyReactiveProperty<Vector2Int> _position;
        private ReadOnlyReactiveProperty<float> _range;
        private HashSet<Vector2Int> _visibleArea = new();
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range, IMap world)
        {
            _position = position;
            _range = range;
            _position.Subscribe(currentPosition => ChangeVisibleArea(Calc(currentPosition, world, _range.CurrentValue)));
            _range.Subscribe(range => ChangeVisibleArea(Calc(_position.CurrentValue, world, range)));
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;

        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Refresh(IMap world)
        {
            ChangeVisibleArea(Calc(_position.CurrentValue, world, _range.CurrentValue));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            var oldArea = _visibleArea;
            _visibleArea = area;
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, oldArea));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IMap world, float range)
        {
            return ViewCalculator.ComputeCircle(world.GetAllLightPassablePositions(), position, range);
        }
    }
}