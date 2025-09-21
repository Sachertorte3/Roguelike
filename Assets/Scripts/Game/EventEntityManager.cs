#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Domain.Service.Events;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        public readonly List<Stairs> Stairs = new();
        private readonly List<Chest> _chests = new();
        private readonly List<Trap> _traps = new();
        public readonly List<Statue> Statues = new();
        private readonly List<Money> _money = new();
        private Option<Bonfire> _bonfire = Option<Bonfire>.None;
        private Option<MagicPot> _magicPot = Option<MagicPot>.None;
        private Option<Teleporter> _teleporter = Option<Teleporter>.None;
        private ObservableList<IEventEntity> _standaloneEventEntities = new();
        private ObservableList<IPlayerEventEntity> _standalonePlayerEventEntities = new();
        private ObservableList<IScheduledEventEntity> _standaloneScheduledEventEntities = new();

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

            foreach (var statueMemento in eventEntities.Statues)
            {
                var statue = new Statue(statueMemento);
                Statues.Add(statue);
                Spawn(statue);
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

            _teleporter = eventEntities.Teleporter.Map(teleporter => new Teleporter(teleporter));
            if (_teleporter.HasValue)
                Spawn(_teleporter.Value!);

            _standaloneEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
            _standalonePlayerEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
            _standaloneScheduledEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
        }

        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento
            (
                Stairs.Select(stairs => stairs.Serialize()).ToList(),
                _chests.Select(chest => chest.Serialize()).ToList(),
                _traps.Select(trap => trap.Serialize()).ToList(),
                Statues.Select(statue => statue.Serialize()).ToList(),
                _money.Select(money => money.Serialize()).ToList(),
                _bonfire.Map(bonfire => bonfire.Serialize()),
                _magicPot.Map(magicPot => magicPot.Serialize()),
                _teleporter.Map(teleporter => teleporter.Serialize())
            );
        }

        public static EventEntitiesMemento Build(
            IEnumerable<StairsMemento> stairs,
            IEnumerable<ChestMemento> chests,
            IEnumerable<TrapMemento> traps,
            IEnumerable<StatueMemento> statues,
            IEnumerable<MoneyMemento> money,
            Option<BonfireMemento> bonfire,
            Option<MagicPotMemento> magicPot,
            Option<EntityMemento> teleporter
        )
        {
            return new EventEntitiesMemento
            (
                stairs.ToList(),
                chests.ToList(),
                traps.ToList(),
                statues.ToList(),
                money.ToList(),
                bonfire,
                magicPot,
                teleporter
            );
        }

        public IObservableCollection<IEventEntity> StandaloneEventEntities => _standaloneEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities =>
            _standalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => _standaloneScheduledEventEntities;

        public void Spawn(IEventEntity eventEntity)
        {
            _standaloneEventEntities.Add(eventEntity);
        }

        public void Spawn(IPlayerEventEntity eventEntity)
        {
            _standalonePlayerEventEntities.Add(eventEntity);
        }

        public void Spawn(IScheduledEventEntity eventEntity)
        {
            _standaloneScheduledEventEntities.Add(eventEntity);
        }

        public void Remove(IEventEntity eventEntity)
        {
            Debug.Log($"Remove event entity: {eventEntity.GetType()}");
            _standaloneEventEntities.Remove(eventEntity);
            if (eventEntity is Trap trap)
            {
                _traps.Remove(trap);
            }
            else if (eventEntity is Money money)
            {
                _money.Remove(money);
            }
            else if (eventEntity is Teleporter)
            {
                _teleporter = Option<Teleporter>.None;
            }
            else
            {
                throw new Exception($"Unknown event entity: {eventEntity.GetType()}");
            }
        }

        public void Remove(IPlayerEventEntity eventEntity)
        {
            _standalonePlayerEventEntities.Remove(eventEntity);
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
            else if (eventEntity is MagicPot)
            {
                _magicPot = Option<MagicPot>.None;
            }
            else
            {
                throw new Exception($"Unknown event entity: {eventEntity.GetType()}");
            }
        }

        public void Remove(IScheduledEventEntity eventEntity)
        {
            _standaloneScheduledEventEntities.Remove(eventEntity);
            if (eventEntity is Statue statue)
            {
                Statues.Remove(statue);
            }
            else
            {
                throw new Exception($"Unknown event entity: {eventEntity.GetType()}");
            }
        }
    }
}