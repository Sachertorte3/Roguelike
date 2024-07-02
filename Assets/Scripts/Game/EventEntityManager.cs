#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Rooms;
using ObservableCollections;

namespace Model.Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        private readonly UpStairs? _upStairs;
        private readonly DownStairs _downStairs;
        private readonly List<Chest> _chests = new();
        private readonly List<Clerk> _clerks = new();
        private ObservableList<IEventEntity> _eventEntities = new();
        private ObservableList<IEventEntityAndIcon> _eventEntitiesAndIcons = new();

        public EventEntityManager(EventEntitiesMemento eventEntities)
        {
            _downStairs = new(eventEntities.DownStairs);
            _eventEntitiesAndIcons.Add(_downStairs);

            if (eventEntities.UpStairs != null)
            {
                _upStairs = new(eventEntities.UpStairs);
                _eventEntitiesAndIcons.Add(_upStairs);
            }
            else
            {
                _upStairs = null;
            }

            foreach (var chest in eventEntities.Chests)
                Add(new Chest(chest));
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
        public IObservableCollection<IEventEntityAndIcon> EventEntitiesAndIcons => _eventEntitiesAndIcons;

        public void Add(Chest chest)
        {
            _chests.Add(chest);
            _eventEntities.Add(chest);
            _eventEntitiesAndIcons.Add(chest);
        }

        public void Add(Clerk clerk)
        {
            _clerks.Add(clerk);
            _eventEntities.Add(clerk);
        }

        public void Remove(Chest chest)
        {
            _chests.Remove(chest);
            _eventEntities.Remove(chest);
            _eventEntitiesAndIcons.Remove(chest);
        }
    }
}