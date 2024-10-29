using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
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
        public readonly FlagStat ClairvoyantFlags;
        public readonly FlagStat BlindFlags;
        private bool _canThroughWalls;
        public bool IsClairvoyant => ClairvoyantFlags.CurrentValue;
        public bool IsBlind => BlindFlags.CurrentValue;
        private Subject<Unit> _onVisibleAreaChanged = new();
        private readonly IMap _map;

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range,
            int clairvoyantFlags, int blindFlags, bool canThroughWalls, IMap map)
        {
            _position = position;
            _range = range;
            ClairvoyantFlags = new FlagStat(clairvoyantFlags);
            BlindFlags = new FlagStat(blindFlags);
            _canThroughWalls = canThroughWalls;
            _position.Subscribe(currentPosition =>
                ChangeVisibleArea());
            _range.Subscribe(_ => ChangeVisibleArea());
            ClairvoyantFlags
                .Value
                .Subscribe(_ => ChangeVisibleArea());
            BlindFlags
                .Value
                .Subscribe(_ => ChangeVisibleArea());
            _map = map;
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea
        {
            get
            {
                return Calc(_position.CurrentValue);
            }
        }
        public Observable<Unit> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Refresh()
        {
            ChangeVisibleArea();
        }

        private void ChangeVisibleArea()
        {
            _onVisibleAreaChanged.OnNext(Unit.Default);
        }

        public bool IsVisible(Vector2Int position)
        {
            return _map.IsVisible(_position.CurrentValue, position, _range.CurrentValue + 0.5f);
        }

        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            var range = _range.CurrentValue + 0.5f;
            if (_canThroughWalls)
            {
                if (IsClairvoyant)
                    return _map.GetAllPositions();
                var viewRadiusSq = IsBlind ? 1.5f * 1.5f : range * range;
                return _map.GetAllPositions().Where(
                    pos => (position - pos).sqrMagnitude <= viewRadiusSq).ToHashSet();
            }
            if (IsClairvoyant)
                return _map.GetFullVisibleArea();
            if (IsBlind)
                return _map.GetVisibleArea(position, 1.5f);
            return _map.GetVisibleArea(position, range);
        }
    }
}