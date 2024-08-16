using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterStatusMemento
    {
        public CharacterStatsMemento Stats;
        public int ClairvoyantFlags;
        public ConditionMemento[] Conditions;
    }
}