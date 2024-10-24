using System;
using Sirenix.OdinInspector;
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
        public float EvaluateTurn()
        {
            float estimatedTurns = float.MaxValue;

            if (RemoveByElapsedTurn)
            {
                estimatedTurns = Duration;
            }

            if (RemoveByDamage)
            {
                float damageTurns = 1 / Probability;
                if (damageTurns < estimatedTurns)
                {
                    estimatedTurns = damageTurns;
                }
            }

            if (RemoveByEnemyNearby)
            {
                estimatedTurns = 0;
            }

            return estimatedTurns;
        }
    }
}