using System;
using Domain.Model.Effect;

namespace Domain.Model.Memento
{
    [Serializable]
    public class AffiliationMemento
    {
        public CharacterGroup Group;
        public SerializableDictionary<int, float> Affiliations;
    }
}