using System;
using Domain.Model.Effect;

namespace Domain.Model.Character
{
    [Serializable]
    public class AffiliationMemento
    {
        public CharacterGroup Group;
        public SerializableDictionary<int, float> Affiliations;
    }
}