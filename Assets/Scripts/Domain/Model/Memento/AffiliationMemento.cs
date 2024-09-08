using System;
using Domain.Model.Character;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class AffiliationMemento
    {
        public CharacterGroup Group;
        public SerializableDictionary<string, float> Affiliations;
        public SerializableDictionary<string, AffiliationType> ForcedAffiliations;
    }
}