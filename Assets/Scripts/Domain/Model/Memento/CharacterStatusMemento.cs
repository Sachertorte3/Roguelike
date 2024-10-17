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
        [field: SerializeField] public int CannotActFlags;
        [field: SerializeField] public int CannotMoveFlags;
        [field: SerializeField] public int ConfusedFlags;
        [field: SerializeField] public int ClairvoyantFlags;
        [field: SerializeField] public int BlindFlags;
        [field: SerializeField] public int OverDriveFlags;
        [field: SerializeField] public int HardFlags;
        [field: SerializeField] public int HeavyFlags;
        [field: SerializeField] public int SecureHoldFlags;
        [field: SerializeField] public int CurseProofFlags;
        [field: SerializeField] public int IsAffectedByTrapFlags;
        [SerializeField] private List<ConditionMemento> _conditions;
        [SerializeField] private List<string> _inflicters;

        public List<(Id<IEntity> actor, ConditionMemento condition)> Conditions =>
            _conditions.Select((x, i) => (new Id<IEntity>(_inflicters[i]), x)).ToList();

        public CharacterStatusMemento(CharacterStatsMemento stats, int cannotActFlags, int cannotMoveFlags, int confusedFlags, int clairvoyantFlags, int blindFlags,
            int overDriveFlags, int hardFlags, int heavyFlags, int secureHoldFlags, int isAffectedByTrapFlags, int curseProofFlags, List<(Id<IEntity> actor, ConditionMemento condition)> conditions)
        {
            Stats = stats;
            CannotActFlags = cannotActFlags;
            CannotMoveFlags = cannotMoveFlags;
            ConfusedFlags = confusedFlags;
            ClairvoyantFlags = clairvoyantFlags;
            BlindFlags = blindFlags;
            OverDriveFlags = overDriveFlags;
            HardFlags = hardFlags;
            HeavyFlags = heavyFlags;
            SecureHoldFlags = secureHoldFlags;
            CurseProofFlags = curseProofFlags;
            IsAffectedByTrapFlags = isAffectedByTrapFlags;
            _conditions = conditions.Select(x => x.condition).ToList();
            _inflicters = conditions.Select(x => x.actor.ToString()).ToList();
        }
    }
}