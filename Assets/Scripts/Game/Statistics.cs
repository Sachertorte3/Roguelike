#nullable enable
using System;
using System.Linq;
using Domain.Model;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;

namespace Game
{
    public class Statistics : ISerializable<StatisticsMemento>
    {
        public TimeSpan LastSavePlayTime { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public TimeSpan CurrentSessionTime => DateTime.Now - SessionStartTime;
        public TimeSpan PlayTime => LastSavePlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _turn;
        public ReadOnlyReactiveProperty<int> Turn => _turn;
        public int MaxMapLevel { get; private set; }
        private readonly ObservableHashSet<string> _knownItemNames;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        public bool IsCheating { get; set; }
        public Statistics(StatisticsMemento memento, GameManager game, World world)
        {
            LastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            SessionStartTime = DateTime.Now;
            _turn = new(memento.Turn);
            MaxMapLevel = memento.MaxMapLevel;
            _knownItemNames = new(memento.KnownItemNames);
            IsCheating = memento.IsCheating;

            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                if (map.Depth > MaxMapLevel)
                    MaxMapLevel = map.Depth;
                map.Player.Character.KnownItemNames.ObserveChanged().Subscribe(item =>
                {
                    _knownItemNames.Add(item.NewItem);
                });
            });
            game.OnTurnChanged.Skip(1).Subscribe(_ =>
            {
                _turn.Value++;
            });
        }
        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, _turn.Value, MaxMapLevel, _knownItemNames.ToList(), IsCheating);
        }
        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, 1, new(), false);
        }
    }
}