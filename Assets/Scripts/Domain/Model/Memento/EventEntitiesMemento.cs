using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EventEntitiesMemento
    {
        [field: SerializeField] public List<StairsMemento> Stairs { get; private set; }
        [field: SerializeField] public List<ChestMemento> Chests { get; private set; }
        [field: SerializeField] public List<TrapMemento> Traps { get; private set; }
        [field: SerializeField] public List<MoneyMemento> Money { get; private set; }
        [field: SerializeField] public Option<BonfireMemento> Bonfire { get; private set; }
        [field: SerializeField] public Option<EntityMemento> MagicPot { get; private set; }

        public EventEntitiesMemento(
            List<StairsMemento> stairs,
            List<ChestMemento> chests,
            List<TrapMemento> traps,
            List<MoneyMemento> money,
            Option<BonfireMemento> bonfire,
            Option<EntityMemento> magicPot)
        {
            Stairs = stairs;
            Chests = chests;
            Traps = traps;
            Money = money;
            Bonfire = bonfire;
            MagicPot = magicPot;
        }
    }
}