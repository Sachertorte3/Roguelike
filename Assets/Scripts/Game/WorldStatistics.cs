#nullable enable
using System;
using System.Linq;
using Domain.Model;
using Domain.Model.Memento;
using R3;

namespace Game
{
    public class WorldStatistics : ISerializable<StatisticsMemento>
    {
        public TimeSpan LastSavePlayTime { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public TimeSpan CurrentSessionTime => DateTime.Now - SessionStartTime;
        public TimeSpan PlayTime => LastSavePlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _turn;
        public ReadOnlyReactiveProperty<int> Turn => _turn;
        public int MaxMapLevel { get; private set; }
        public bool IsCheating { get; set; }
        public WorldStatistics(StatisticsMemento memento, GameManager game, World world)
        {
            LastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            SessionStartTime = DateTime.Now;
            _turn = new(memento.Turn);
            MaxMapLevel = memento.MaxMapLevel;
            IsCheating = memento.IsCheating;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var map = mapChanged.Map;
                if (map.Depth > MaxMapLevel)
                    MaxMapLevel = map.Depth;
            });
            game.OnTurnChanged.Skip(1).Subscribe(_ =>
            {
                _turn.Value++;
            });
        }
        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, _turn.Value, MaxMapLevel, IsCheating);
        }
        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, 1, false);
        }
    }
}