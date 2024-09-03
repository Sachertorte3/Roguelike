using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterStatusMemento
    {
        public CharacterStatsMemento Stats;
        public int ClairvoyantFlags;
        public int OverDriveFlags;
        public ConditionMemento[] Conditions;
    }
}