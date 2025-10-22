using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StairsMemento
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [SerializeField] private string _destination;
        public Id<IMap> Destination => new(_destination);
        [SerializeField] private string _destinationId;
        public Id<IEntity> DestinationId => new(_destinationId);
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [SerializeField] private List<string> _keyCharacters;
        public List<Id<IEntity>> KeyCharacters => _keyCharacters.Select(keyCharacter => new Id<IEntity>(keyCharacter)).ToList();

        public StairsMemento(
            MovementEntityType type,
            Id<IMap> destination,
            Id<IEntity> destinationId,
            EntityMemento entity,
            List<Id<IEntity>> keyCharacters)
        {
            Type = type;
            _destination = destination.ToString();
            _destinationId = destinationId.ToString();
            Entity = entity;
            _keyCharacters = keyCharacters.Select(keyCharacter => keyCharacter.ToString()).ToList();
        }
    }
}