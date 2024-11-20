#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    public class StatisticsMemento
    {
        [field: SerializeField] public long PlayTime { get; private set; }
        [field: SerializeField] public int Turn { get; private set; }
        [field: SerializeField] public List<string> KnownItemNames { get; private set; }
        public StatisticsMemento(long playTime, int turn, List<string> knownItemNames)
        {
            PlayTime = playTime;
            Turn = turn;
            KnownItemNames = knownItemNames;
        }
    }
}