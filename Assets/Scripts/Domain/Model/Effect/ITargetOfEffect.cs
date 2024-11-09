using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IEntity
    {
        public IStatusManager StatusManager { get; }
        public IVisionRange VisionRange { get; }
        public string GetName(IPlayer player, bool ignoreVisibility = false);
        public float GetStatValue(StatType type);
        public float GetElementAttackMultiplier(Element element);
        public float GetElementDamageRateMultiplier(Element element);
        public float GetConditionResistance(ConditionTemplate condition);
        public IInventory Inventory { get; }
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

        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition);
        public void ClearCondition();
        public void ClearKnownItems(IMap map);
        public void ClearAffiliation(IMap map);
        public void ListenToAlert(Vector2Int position);
        public void DropItem(int itemIndex, IMap map, bool isForced);
    }
}