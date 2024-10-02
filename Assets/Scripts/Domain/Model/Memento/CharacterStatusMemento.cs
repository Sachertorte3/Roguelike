using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterStatusMemento
    {
        [field: SerializeField] public CharacterStatsMemento Stats;
        [field: SerializeField] public int ClairvoyantFlags;
        [field: SerializeField] public int BlindFlags;
        [field: SerializeField] public int OverDriveFlags;
        [SerializeField] private List<ConditionMemento> _conditions;
        [SerializeField] private List<string> _inflicters;
        public List<(Id<IEntity> actor, ConditionMemento condition)> Conditions => _conditions.Select((x, i) => (new Id<IEntity>(_inflicters[i]), x)).ToList();

        public CharacterStatusMemento(CharacterStatsMemento stats, int clairvoyantFlags, int blindFlags, int overDriveFlags, List<(Id<IEntity> actor, ConditionMemento condition)> conditions)
        {
            Stats = stats;
            ClairvoyantFlags = clairvoyantFlags;
            BlindFlags = blindFlags;
            OverDriveFlags = overDriveFlags;
            _conditions = conditions.Select(x => x.condition).ToList();
            _inflicters = conditions.Select(x => x.actor.ToString()).ToList();
        }
    }
}