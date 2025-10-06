using Domain.Model.Effect;
using R3;

namespace Domain.Model.Character.Status
{
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<float> WaitTimeValue { get; }
        public bool IsFlagStat(FlagStatType type);
        public ReadOnlyReactiveProperty<bool> GetFlagProperty(FlagStatType type);
        public float GetStatValue(StatType type);
        public float GetElementAttackMultiplier(Element element);
        public float GetElementDamageRateMultiplier(Element element);
        public float GetConditionResistance(ConditionTemplate condition);
    }
}