#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Memento;
using Domain.Service.Events;
using Domain.Service.Items;
using ObservableCollections;
using R3;

namespace Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        public readonly List<Stairs> Stairs = new();
        private readonly List<Chest> _chests = new();
        private Option<Bonfire> _bonfire = Option<Bonfire>.None;
        private ObservableList<IEventEntity> _eventEntities = new();
        private ObservableList<IEventEntity> _standaloneEventEntities = new();
        public EventEntityEvents EventEntityEvents = new();

        public EventEntityManager(EventEntitiesMemento eventEntities, ReadOnlyReactiveProperty<bool> isLockedStairs)
        {
            foreach (var stairsMemento in eventEntities.Stairs)
            {
                var stairs = new Stairs(stairsMemento, isLockedStairs);
                Stairs.Add(stairs);
                Spawn(stairs);
            }

            foreach (var chestMemento in eventEntities.Chests)
            {
                var chest = new Chest(chestMemento);
                _chests.Add(chest);
                Spawn(chest);
            }

            _bonfire = eventEntities.Bonfire.Map(bonfire => new Bonfire(bonfire));
            if (_bonfire.HasValue)
                Spawn(_bonfire.Value!);

            EventEntityEvents.OnDestroyed.Subscribe(destroyed => Remove(destroyed.EventEntity));
        }
        public static EventEntitiesMemento Build(IEnumerable<StairsMemento> stairs, IEnumerable<ChestMemento> chests, Option<EntityMemento> bonfire)
        {
            return new EventEntitiesMemento
            (
                stairs: stairs.ToList(),
                chests: chests.ToList(),
                bonfire: bonfire
            );
        }
        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento
            (
                stairs: Stairs.Select(stairs => stairs.Serialize()).ToList(),
                chests: _chests.Select(chest => chest.Serialize()).ToList(),
                bonfire: _bonfire.Map(bonfire => bonfire.Serialize())
            );
        }

        public IObservableCollection<IEventEntity> EventEntities => _eventEntities;
        public IObservableCollection<IEventEntity> StandaloneEventEntities => _standaloneEventEntities;

        public void Spawn(IEventEntity eventEntity)
        {
            _standaloneEventEntities.Add(eventEntity);
            Add(eventEntity);
        }

        public void Add(IEventEntity eventEntity)
        {
            _eventEntities.Add(eventEntity);
            EventEntityEvents.Add(eventEntity);
        }

        public void Remove(IEventEntity eventEntity)
        {
            _eventEntities.Remove(eventEntity);
            _standaloneEventEntities.Remove(eventEntity);
            if (eventEntity is Chest chest)
            {
                _chests.Remove(chest);
            }
            else if (eventEntity is Stairs stairs)
            {
                Stairs.Remove(stairs);
            }
            else if (eventEntity is Bonfire)
            {
                _bonfire = Option<Bonfire>.None;
            }
            EventEntityEvents.Remove(eventEntity);
        }
    }
}