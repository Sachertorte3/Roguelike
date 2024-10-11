using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Message;
using R3;
using Stats;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private ReadOnlyReactiveProperty<Vector2Int> _position;
        private ReadOnlyReactiveProperty<float> _range;
        private readonly FlagStat _clairvoyantFlags;
        private readonly FlagStat _blindFlags;
        private bool _canThroughWalls;
        public bool IsClairvoyant => _clairvoyantFlags.CurrentValue;
        public bool IsBlind => _blindFlags.CurrentValue;
        private HashSet<Vector2Int> _visibleArea = new();
        private Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range,
            int clairvoyantFlags, int blindFlags, bool canThroughWalls, IMap map)
        {
            _position = position;
            _range = range;
            _clairvoyantFlags = new FlagStat(clairvoyantFlags);
            _blindFlags = new FlagStat(blindFlags);
            _canThroughWalls = canThroughWalls;
            _position.Subscribe(currentPosition =>
                ChangeVisibleArea(Calc(currentPosition, map)));
            _range.Subscribe(range => ChangeVisibleArea(Calc(_position.CurrentValue, map)));
            _clairvoyantFlags
                .Value
                .Subscribe(_ => ChangeVisibleArea(Calc(_position.CurrentValue, map)));
            _blindFlags
                .Value
                .Subscribe(_ => ChangeVisibleArea(Calc(_position.CurrentValue, map)));
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;
        public int ClairvoyantFlags => _clairvoyantFlags.CurrentFlags;
        public int BlindFlags => _blindFlags.CurrentFlags;
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void AddClairvoyantFlags()
        {
            _clairvoyantFlags.AddFlags();
        }

        public void RemoveClairvoyantFlags()
        {
            _clairvoyantFlags.RemoveFlags();
        }

        public void AddBlindFlags()
        {
            _blindFlags.AddFlags();
        }

        public void RemoveBlindFlags()
        {
            _blindFlags.RemoveFlags();
        }

        public void Refresh(IMap map)
        {
            ChangeVisibleArea(Calc(_position.CurrentValue, map));
        }

        private void ChangeVisibleArea(HashSet<Vector2Int> area)
        {
            var oldArea = _visibleArea;
            _visibleArea = area;
            _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(area, oldArea));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position, IMap map)
        {
            var range = _range.CurrentValue;
            if (_canThroughWalls)
            {
                if (IsClairvoyant)
                    return map.GetAllPositions();
                var viewRadiusSq = IsBlind ? 1.5f * 1.5f : range * range;
                return map.GetAllPositions().Where(
                    pos => (position - pos).sqrMagnitude <= viewRadiusSq).ToHashSet();
            }
            if (IsClairvoyant)
                return ViewCalculator.ComputeFullVisibility(map.GetAllLightPassablePositions());
            if (IsBlind)
                return ViewCalculator.ComputeCircle(map.GetAllLightPassablePositions(), position, 1.5f);
            return ViewCalculator.ComputeCircle(map.GetAllLightPassablePositions(), position, range);
        }
    }
}