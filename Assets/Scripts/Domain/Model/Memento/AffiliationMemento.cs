using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class AffiliationMemento
    {
        [field: SerializeField] public CharacterGroup Group { get; private set; }
        [SerializeField] private SerializableDictionary<string, float> _affiliations;
        public Dictionary<Id<IEntity>, float> Affiliations => _affiliations.ToDictionary(x => new Id<IEntity>(x.Key), x => x.Value);
        [SerializeField] private SerializableDictionary<string, AffiliationType> _forcedAffiliations;
        public Dictionary<Id<IEntity>, AffiliationType> ForcedAffiliations => _forcedAffiliations.ToDictionary(x => new Id<IEntity>(x.Key), x => x.Value);
        public AffiliationMemento(CharacterGroup group, Dictionary<Id<IEntity>, float> affiliations, Dictionary<Id<IEntity>, AffiliationType> forcedAffiliations)
        {
            Group = group;
            _affiliations = affiliations.ToSerializableDictionary(x => x.Key.ToString(), x => x.Value);
            _forcedAffiliations = forcedAffiliations.ToSerializableDictionary(x => x.Key.ToString(), x => x.Value);
        }
    }
}