#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Service.Events;
using ObservableCollections;

namespace Model.Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        private readonly UpStairs? _upStairs;
        private readonly DownStairs _downStairs;
        private readonly List<Chest> _chests = new();
        private ObservableList<IEventEntity> _eventEntities = new();

        public EventEntityManager(EventEntitiesMemento eventEntities)
        {
            _downStairs = new(eventEntities.DownStairs);
            _eventEntities.Add(_downStairs);

            if (eventEntities.UpStairs != null)
            {
                _upStairs = new(eventEntities.UpStairs);
                _eventEntities.Add(_upStairs);
            }
            else
            {
                _upStairs = null;
            }

            foreach (var chest in eventEntities.Chests)
                Add(new(chest));
        }
        public static EventEntitiesMemento Build(DownStairsMemento downStairs, UpStairsMemento? upStairs, IEnumerable<ChestMemento> chests)
        {
            return new EventEntitiesMemento(
                downStairs,
                upStairs,
                chests.ToList()
            );
        }
        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento(
                _downStairs.Serialize(),
                _upStairs?.Serialize(),
                _chests.Select(chest => chest.Serialize()).ToList()
            );
        }

        public IObservableCollection<IEventEntity> EventEntities => _eventEntities;

        public void Add(Chest chest)
        {
            _chests.Add(chest);
            _eventEntities.Add(chest);
        }

        public void Remove(Chest chest)
        {
            _chests.Remove(chest);
            _eventEntities.Remove(chest);
        }
    }
}