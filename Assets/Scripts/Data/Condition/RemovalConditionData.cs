using Sirenix.OdinInspector;
using System;
using Random = UnityEngine.Random;

namespace Data.Condition
{
    [Serializable]
    public class RemovalConditionData
    {
        public bool RemoveByElapsedTurn = false;
        [ShowIf("@RemoveByElapsedTurn")] public int Duration;
        public bool RemoveByDamage = false;
        [ShowIf("@RemoveByDamage")] public int AcceptableDamage;
        [ShowIf("@RemoveByDamage")] public float Probability;

        public RemovalConditionData(int duration = -1, int acceptableDamage = 0, float probability = -1)
        {
            if (duration > 0)
            {
                RemoveByElapsedTurn = true;
                Duration = duration;
            }

            if (acceptableDamage >= 0 && probability > 0)
            {
                RemoveByDamage = true;
                AcceptableDamage = acceptableDamage;
                Probability = probability;
            }
        }

        public bool IsFinished(int elapsedTurns, int receivedDamage)
        {
            return (RemoveByElapsedTurn && elapsedTurns >= Duration) ||
                   (RemoveByDamage && receivedDamage > AcceptableDamage && Random.value < Probability);
        }
    }
}
