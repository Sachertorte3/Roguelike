#nullable enable
using System.Linq;
using Domain.Model.Memento;
using ObservableCollections;
using R3;

namespace Game
{
    public class GlobalStatistics
    {
        public int MaxMapLevel { get; private set; }
        private readonly ObservableHashSet<string> _knownItemNames;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        public GlobalStatistics(GlobalStatisticsMemento memento, World world)
        {
            _knownItemNames = new(memento.KnownItemNames);

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var map = mapChanged.Map;
                if (map.Depth > MaxMapLevel)
                    MaxMapLevel = map.Depth;
                map.Player.Character.KnownItemNames.ObserveChanged().Subscribe(item =>
                {
                    _knownItemNames.Add(item.NewItem);
                });
            });
        }
        public GlobalStatisticsMemento Serialize()
        {
            return new GlobalStatisticsMemento(MaxMapLevel, _knownItemNames.ToList());
        }
        public static GlobalStatisticsMemento Build()
        {
            return new GlobalStatisticsMemento(1, new());
        }
    }
}