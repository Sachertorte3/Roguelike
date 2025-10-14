#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    public class GlobalStatisticsMemento
    {
        [field: SerializeField] public int MaxMapLevel { get; private set; }
        [field: SerializeField] public List<string> KnownItemNames { get; private set; }
        public GlobalStatisticsMemento(
            int maxMapLevel,
            List<string> knownItemNames)
        {
            MaxMapLevel = maxMapLevel;
            KnownItemNames = knownItemNames;
        }
    }
}