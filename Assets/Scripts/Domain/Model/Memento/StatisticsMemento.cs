#nullable enable
using UnityEngine;

namespace Domain.Model.Memento
{
    public class StatisticsMemento
    {
        [field: SerializeField] public long PlayTime { get; private set; }
        [field: SerializeField] public int Turn { get; private set; }
        [field: SerializeField] public int MaxMapLevel { get; private set; }
        [field: SerializeField] public bool IsCheating { get; private set; }
        public StatisticsMemento(
            long playTime,
            int turn,
            int maxMapLevel,
            bool isCheating)
        {
            PlayTime = playTime;
            Turn = turn;
            MaxMapLevel = maxMapLevel;
            IsCheating = isCheating;
        }
    }
}