using System;
using System.Linq;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Domain.Model.Condition
{
    [Serializable]
    public class RemovalConditionData
    {
        public bool RemoveByElapsedTurn = false;
        [ShowIf("@RemoveByElapsedTurn")] public int Duration;
        public bool RemoveByDamage = false;
        [ShowIf("@RemoveByDamage")] public float Probability;
        public bool RemoveByEnemyNearby = false;

        public RemovalConditionData(int duration = -1, float probability = -1, bool removeByEnemyNearby = false)
        {
            if (duration > 0)
            {
                RemoveByElapsedTurn = true;
                Duration = duration;
            }

            if (probability > 0)
            {
                RemoveByDamage = true;
                Probability = probability;
            }

            RemoveByEnemyNearby = removeByEnemyNearby;
        }

        public bool IsFinished(int elapsedTurns, bool enemyVisible)
        {
            return (RemoveByElapsedTurn && elapsedTurns >= Duration) ||
                   (RemoveByDamage && Random.value < Probability) ||
                   (RemoveByEnemyNearby && enemyVisible);
        }
    }
}