using System;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Domain.Model.Condition
{
    [Serializable]
    public class RemovalConditionData
    {
        public bool RemoveByElapsedTurn;
        [ShowIf("@RemoveByElapsedTurn")] public int Duration;
        public bool RemoveByDamage;
        [ShowIf("@RemoveByDamage")] public float Probability;
        public bool RemoveByCharacterNearby;

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

            RemoveByCharacterNearby = removeByEnemyNearby;
        }

        public bool IsFinished(int elapsedTurns, bool characterVisible)
        {
            return (RemoveByElapsedTurn && elapsedTurns >= Duration) ||
                   (RemoveByCharacterNearby && characterVisible);
        }

        public bool IsFinishedByDamage()
        {
            return RemoveByDamage && Random.value < Probability;
        }

        public float EvaluateTurn()
        {
            var estimatedTurns = float.MaxValue;

            if (RemoveByElapsedTurn)
            {
                estimatedTurns = Duration;
            }

            if (RemoveByDamage)
            {
                var damageTurns = 1 / Probability;
                if (damageTurns < estimatedTurns)
                {
                    estimatedTurns = damageTurns;
                }
            }

            if (RemoveByCharacterNearby)
            {
                estimatedTurns = 0;
            }

            return estimatedTurns;
        }
    }
}