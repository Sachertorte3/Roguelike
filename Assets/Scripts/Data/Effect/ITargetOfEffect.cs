using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Condition;
using UnityEngine;
using Utilities;

namespace Data.Effect
{
    public interface ITargetOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }

        /// <summary>
        /// Takes damage
        /// </summary>
        /// <param name="value">The amount of damage to take</param>
        /// <returns>The actual amount of HP reduced</returns>
        public UniTask<int> LoseHp(int value);

        /// <summary>
        /// Recovers HP
        /// </summary>
        /// <param name="value">The amount of HP to recover</param>
        /// <returns>The actual amount of HP recovered</returns>
        public UniTask<int> GainHp(int value);

        public UniTask BlowAway(Direction8 direction, int distance, IPassableChecker map);
        public void Teleport(Vector2Int position);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);
    }
}