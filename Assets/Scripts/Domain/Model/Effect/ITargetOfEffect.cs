using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IEntity
    {
        public string GetName(IHasAffiliation player);
        public float GetStatValue(StatType type);
        public bool IsClairvoyant { get; }
        public bool IsConfused { get; }
        public bool CanAct { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }

        /// <summary>
        /// Takes damage
        /// </summary>
        /// <param name="value">The amount of damage to take</param>
        /// <returns>The actual amount of HP reduced</returns>
        public int LoseHp(int value);

        /// <summary>
        /// Recovers HP
        /// </summary>
        /// <param name="value">The amount of HP to recover</param>
        /// <returns>The actual amount of HP recovered</returns>
        public int GainHp(int value);

        public UniTask BlowAway(Direction8 direction, int distance, IPassableChecker map);
        public void Teleport(Vector2Int position);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);
        public void RepairAllItem();
    }
}