#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Events;
using Domain.Service.Items;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Game
{
    public class FireEntityManager
    {
        private readonly ObservableList<Fire> _fireEntities = new();
        public FireEntityEvents EntityEvents = new();

        public FireEntityManager(FireEntitiesMemento memento)
        {
            foreach (var fireMemento in memento.Fires)
            {
                Add(new Fire(fireMemento));
            }
            EntityEvents.OnDestroyed.Subscribe(destroyed => Remove(destroyed.Entity));
        }

        public FireEntitiesMemento Serialize()
        {
            return new FireEntitiesMemento(_fireEntities.Select(fire => fire.Serialize()).ToList());
        }

        public static FireEntitiesMemento Build()
        {
            return new FireEntitiesMemento(new());
        }

        public IObservableCollection<Fire> FireEntities => _fireEntities;

        public void Add(Fire entity)
        {
            if (!_fireEntities.Any(fire => entity.CurrentPosition == fire.CurrentPosition))
            {
                _fireEntities.Add(entity);
                EntityEvents.Add(entity);
            }
        }

        public void Remove(Fire entity)
        {
            _fireEntities.Remove(entity);
            EntityEvents.Remove(entity);
        }

        public void UpdateTurn(IMap map)
        {
            var destroyedFires = new List<Fire>();
            var addedFires = new List<Fire>();
            foreach (var fire in _fireEntities)
            {
                if (Random.value < 1 / 4f)
                {
                    destroyedFires.Add(fire);
                }
                var positions = DirectionMethods
                    .AllDirections
                    .Select(direction => fire.CurrentPosition + direction.Vector())
                    .Where(position => map.At(position).CanPlace(false, false, true));
                foreach (var position in positions)
                {
                    if (Random.value < GetProbabilityOfFireSpreading(position, map))
                    {
                        addedFires.Add(new Fire(Fire.Build(position)));
                    }
                }
            }
            foreach (var fire in destroyedFires)
            {
                fire.Destroy();
            }
            foreach (var fire in addedFires)
            {
                Add(fire);
            }
        }
        public float GetProbabilityOfFireSpreading(Vector2Int position, IMap map)
        {
            var value = 1 / 64f;
            if (map.IsGrass(position))
                value += 1 / 16f;
            var entity = map.Entities.At(position).FirstOrDefault();
            if (entity != null)
                value += 1 / 32f;
            return value;
        }
    }
}