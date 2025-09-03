#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Domain.Service.Events;
using ObservableCollections;
using R3;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        public readonly List<Stairs> Stairs = new();
        private readonly List<Chest> _chests = new();
        private readonly List<Trap> _traps = new();
        private readonly List<Money> _money = new();
        private Option<Bonfire> _bonfire = Option<Bonfire>.None;
        private Option<MagicPot> _magicPot = Option<MagicPot>.None;
        private ObservableList<IEventEntity> _eventEntities = new();
        private ObservableList<IEventEntity> _standaloneEventEntities = new();
        private ObservableList<IPlayerEventEntity> _playerEventEntities = new();
        private ObservableList<IPlayerEventEntity> _standalonePlayerEventEntities = new();

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

            foreach (var trapMemento in eventEntities.Traps)
            {
                var trap = new Trap(trapMemento);
                _traps.Add(trap);
                Spawn(trap);
            }

            foreach (var moneyMemento in eventEntities.Money)
            {
                var money = new Money(moneyMemento);
                _money.Add(money);
                Spawn(money);
            }

            _bonfire = eventEntities.Bonfire.Map(bonfire => new Bonfire(bonfire));
            if (_bonfire.HasValue)
                Spawn(_bonfire.Value!);

            _magicPot = eventEntities.MagicPot.Map(magicPot => new MagicPot(magicPot));
            if (_magicPot.HasValue)
                Spawn(_magicPot.Value!);

            _eventEntities.SubscribeIncludingCurrentObservables(
                eventEntity => eventEntity.Entity.OnDestroyed,
                (eventEntity, _) => Remove(eventEntity)
            );
            _playerEventEntities.SubscribeIncludingCurrentObservables(
                eventEntity => eventEntity.Entity.OnDestroyed,
                (eventEntity, _) => Remove(eventEntity)
            );
        }

        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento
            (
                Stairs.Select(stairs => stairs.Serialize()).ToList(),
                _chests.Select(chest => chest.Serialize()).ToList(),
                _traps.Select(trap => trap.Serialize()).ToList(),
                _money.Select(money => money.Serialize()).ToList(),
                _bonfire.Map(bonfire => bonfire.Serialize()),
                _magicPot.Map(magicPot => magicPot.Serialize())
            );
        }

        public static EventEntitiesMemento Build(IEnumerable<StairsMemento> stairs, IEnumerable<ChestMemento> chests,
            IEnumerable<TrapMemento> traps, IEnumerable<MoneyMemento> money, Option<EntityMemento> bonfire, Option<EntityMemento> magicPot)
        {
            return new EventEntitiesMemento
            (
                stairs.ToList(),
                chests.ToList(),
                traps.ToList(),
                money.ToList(),
                bonfire,
                magicPot
            );
        }

        public IObservableCollection<IEventEntity> EventEntities => _eventEntities;
        public IObservableCollection<IEventEntity> StandaloneEventEntities => _standaloneEventEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => _playerEventEntities;

        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities =>
            _standalonePlayerEventEntities;

        public void Spawn(IEventEntity eventEntity)
        {
            _standaloneEventEntities.Add(eventEntity);
            Add(eventEntity);
        }

        public void Spawn(IPlayerEventEntity eventEntity)
        {
            _standalonePlayerEventEntities.Add(eventEntity);
            Add(eventEntity);
        }

        public void Add(IEventEntity eventEntity)
        {
            _eventEntities.Add(eventEntity);
        }

        public void Add(IPlayerEventEntity eventEntity)
        {
            _playerEventEntities.Add(eventEntity);
        }

        public void Remove(IEventEntity eventEntity)
        {
            _eventEntities.Remove(eventEntity);
            _standaloneEventEntities.Remove(eventEntity);
            if (eventEntity is Trap trap)
            {
                _traps.Remove(trap);
            }
        }

        public void Remove(IPlayerEventEntity eventEntity)
        {
            _playerEventEntities.Remove(eventEntity);
            _standalonePlayerEventEntities.Remove(eventEntity);
            if (eventEntity is Chest chest)
            {
                _chests.Remove(chest);
            }
            else if (eventEntity is Stairs stairs)
            {
                Stairs.Remove(stairs);
            }
            else if (eventEntity is Money money)
            {
                _money.Remove(money);
            }
            else if (eventEntity is Bonfire)
            {
                _bonfire = Option<Bonfire>.None;
            }
            else if (eventEntity is MagicPot)
            {
                _magicPot = Option<MagicPot>.None;
            }
        }
    }
}