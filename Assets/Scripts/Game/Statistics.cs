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
        public ReactiveProperty<int> Turn { get; set; }
        public ObservableHashSet<string> KnownItemNames { get; private set; } = new();
        public Statistics(StatisticsMemento memento, GameManager game, World world)
        {
            LastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            SessionStartTime = DateTime.Now;
            Turn = new(memento.Turn);
            KnownItemNames = new(memento.KnownItemNames);

            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                map.Player.Character.KnownItemNames.ObserveChanged().Subscribe(item =>
                {
                    KnownItemNames.Add(item.NewItem);
                });
            });
            game.OnTurnChanged.Skip(1).Subscribe(_ =>
            {
                Turn.Value++;
            });
        }
        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, Turn.Value, KnownItemNames.ToList());
        }
        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, new());
        }
    }
}