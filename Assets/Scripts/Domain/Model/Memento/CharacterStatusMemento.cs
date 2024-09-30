using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterStatusMemento
    {
        [field: SerializeField] public CharacterStatsMemento Stats;
        [field: SerializeField] public int ClairvoyantFlags;
        [field: SerializeField] public int BlindFlags;
        [field: SerializeField] public int OverDriveFlags;
        [field: SerializeField] public ConditionMemento[] Conditions;
        public CharacterStatusMemento(CharacterStatsMemento stats, int clairvoyantFlags, int blindFlags, int overDriveFlags, ConditionMemento[] conditions)
        {
            Stats = stats;
            ClairvoyantFlags = clairvoyantFlags;
            BlindFlags = blindFlags;
            OverDriveFlags = overDriveFlags;
            Conditions = conditions;
        }
    }
}