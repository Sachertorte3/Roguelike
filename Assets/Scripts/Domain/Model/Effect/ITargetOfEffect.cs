using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Item;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IEntity
    {
        public string GetName(IHasAffiliation player, bool ignoreVisibility = false);
        public float GetStatValue(StatType type);
        public float GetElementAttackMultiplier(Element element);
        public float GetElementDamageRateMultiplier(Element element);
        public IInventory Inventory { get; }
        public bool IsClairvoyant { get; }
        public bool IsOverDrive { get; }
        public bool IsConfused { get; }
        public bool IsHard { get; }
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

        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition);
        public void ClearCondition();
        public void ListenToAlert(IActorOfEffect actor);
    }
}