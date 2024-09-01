using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Message;
using R3;
using Stats;
using UnityEngine;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private ReadOnlyReactiveProperty<Vector2Int> _position;
        private ReadOnlyReactiveProperty<float> _range;
        private readonly FlagStat _clairvoyantFlags;
        public bool IsClairvoyant => _clairvoyantFlags.CurrentValue;
        private HashSet<Vector2Int> _visibleArea = new();
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range, int clairvoyantFlags, IMap world)
        {
            _position = position;
            _range = range;
            _clairvoyantFlags = new FlagStat(clairvoyantFlags);
            _position.Subscribe(currentPosition => ChangeVisibleArea(Calc(currentPosition, world, _range.CurrentValue)));
            _range.Subscribe(range => ChangeVisibleArea(Calc(_position.CurrentValue, world, range)));
            _clairvoyantFlags
                .Value
                .Subscribe(_ => ChangeVisibleArea(Calc(_position.CurrentValue, world, _range.CurrentValue)));
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;
        public int ClairvoyantFlags => _clairvoyantFlags.CurrentFlags;
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void AddClairvoyantFlags()
        {
            _clairvoyantFlags.AddFlags();
        }

        public void RemoveClairvoyantFlags()
        {
            _clairvoyantFlags.RemoveFlags();
        }

        public void Refresh(IMap map)
        {
            ChangeVisibleArea(Calc(_position.CurrentValue, map, _range.CurrentValue));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            var oldArea = _visibleArea;
            _visibleArea = area;
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, oldArea));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IMap map, float range)
        {
            if (IsClairvoyant)
                return ViewCalculator.ComputeFullVisibility(map.GetAllLightPassablePositions());
            else
                return ViewCalculator.ComputeCircle(map.GetAllLightPassablePositions(), position, range);
        }
    }
}