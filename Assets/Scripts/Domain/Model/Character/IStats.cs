using Domain.Model.Condition;
using R3;

namespace Domain.Model.Character
{
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public int CurrentHp { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
        public int CurrentMaxHp { get; }
        public float GetElementAttackMultiplier(Element element);
        public float GetElementDamageRateMultiplier(Element element);
        public float GetConditionResistance(ConditionTemplate condition);
        public ReadOnlyReactiveProperty<float> ViewRangeValue { get; }
        public float CurrentViewRange { get; }
        public float CurrentMaxWaitTime { get; }
        public float CurrentWaitTime { get; }
    }
}