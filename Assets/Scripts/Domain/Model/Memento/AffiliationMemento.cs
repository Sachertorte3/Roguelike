using System;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class AffiliationMemento
    {
        public CharacterGroup Group;
        public SerializableDictionary<Guid, float> Affiliations;
    }
}