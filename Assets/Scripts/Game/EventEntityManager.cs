#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common.Merge;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;

namespace Model.Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        private readonly UpStairs _upStairs;
        public readonly DownStairs DownStairs;
        private readonly List<Chest> _chests = new();
        private ObservableList<IEventEntity> _eventEntities = new();
        private ObservableList<IIconEventEntity> _eventEntitiesAndIcons = new();
        public EventEntityEvents EventEntityEvents = new();

        public EventEntityManager(EventEntitiesMemento eventEntities, ReadOnlyReactiveProperty<bool> isLockedDownStairs)
        {
            DownStairs = new(eventEntities.DownStairs, isLockedDownStairs);
            Add(DownStairs);

            _upStairs = new(eventEntities.UpStairs);
            Add(_upStairs);

            foreach (var chest in eventEntities.Chests)
                Add(new Chest(chest));
            
            EventEntityEvents.OnDestroyed.Subscribe(destroyed => Remove(destroyed.EventEntity));
        }
        public static EventEntitiesMemento Build(DownStairsMemento downStairs, UpStairsMemento? upStairs, IEnumerable<ChestMemento> chests)
        {
            return new EventEntitiesMemento
            {
                DownStairs = downStairs,
                UpStairs = upStairs,
                Chests = chests.ToList()
            };
        }
        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento
            {
                DownStairs = DownStairs.Serialize(),
                UpStairs = _upStairs?.Serialize(),
                Chests = _chests.Select(chest => chest.Serialize()).ToList()
            };
        }

        public IObservableCollection<IEventEntity> EventEntities => _eventEntities;
        public IObservableCollection<IIconEventEntity> EventEntitiesAndIcons => _eventEntitiesAndIcons;

        public void Add(Chest chest)
        {
            _chests.Add(chest);
            _eventEntities.Add(chest);
            _eventEntitiesAndIcons.Add(chest);
            EventEntityEvents.Add(chest);
        }

        public void Add(IEventEntity eventEntity)
        {
            _eventEntities.Add(eventEntity);
            if (eventEntity is IIconEventEntity iconEventEntity)
            {
                _eventEntitiesAndIcons.Add(iconEventEntity);
            }
            EventEntityEvents.Add(eventEntity);
        }

        public void Remove(Chest chest)
        {
            _chests.Remove(chest);
            _eventEntities.Remove(chest);
            _eventEntitiesAndIcons.Remove(chest);
        }

        public void Remove(IEventEntity eventEntity)
        {
            _eventEntities.Remove(eventEntity);
            if (eventEntity is IIconEventEntity eventEntityAndIcon)
            {
                _eventEntitiesAndIcons.Remove(eventEntityAndIcon);
            }
            EventEntityEvents.Remove(eventEntity);
        }
    }
}