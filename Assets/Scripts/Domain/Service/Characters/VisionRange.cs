using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities.Stats;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private ReadOnlyReactiveProperty<Vector2Int> _position;
        private ReadOnlyReactiveProperty<float> _range;
        private readonly IFlagStat _clairvoyantFlags;
        private readonly IFlagStat _blindFlags;
        private readonly Func<bool> _canThroughWalls;
        public bool IsClairvoyant => _clairvoyantFlags.CurrentValue;
        public bool IsBlind => _blindFlags.CurrentValue;
        private Subject<Unit> _onVisibleAreaChanged = new();
        private readonly IMap _map;

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range,
            IFlagStat clairvoyantFlags, IFlagStat blindFlags, Func<bool> canThroughWalls, IMap map)
        {
            _position = position;
            _range = range;
            _canThroughWalls = canThroughWalls;
            _clairvoyantFlags = clairvoyantFlags;
            _blindFlags = blindFlags;
            Observable.Merge(
                _position.AsUnitObservable(),
                _range.AsUnitObservable(),
                _clairvoyantFlags.Value.AsUnitObservable(),
                _blindFlags.Value.AsUnitObservable()
            ).Subscribe(_ =>
            {
                ChangeVisibleArea();
            });
            _map = map;
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => Calc(_position.CurrentValue);
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
            var range = _range.CurrentValue + 0.5f;
            if (_canThroughWalls())
            {
                if (IsClairvoyant)
                    return true;
                var viewRadiusSq = IsBlind ? 1.5f * 1.5f : range * range;
                return (position - _position.CurrentValue).sqrMagnitude <= viewRadiusSq;
            }

            if (IsClairvoyant)
                return true;
            if (IsBlind)
                return _map.IsVisible(_position.CurrentValue, position, 1.5f);
            return _map.IsVisible(_position.CurrentValue, position, range);
        }

        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            var range = _range.CurrentValue + 0.5f;
            if (_canThroughWalls())
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