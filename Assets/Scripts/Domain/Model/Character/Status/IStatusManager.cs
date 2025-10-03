#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using ObservableCollections;
using R3;

namespace Domain.Model.Character.Status
{
    public record OnDamageReceivedMessage(int Damage, string CauseOfDamageLog);
    public interface IStatusManager
    {
        public IStats Stats { get; }
        public Observable<OnDamageReceivedMessage> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<ICondition> Conditions { get; }
        public UniTask UpdateTurn(IHasCondition hasCondition, bool enemyVisible);
        public void AddStatValue(StatType type, float value);
        public void RemoveStatValue(StatType type, float value);
        public void AddStatMultiplier(StatType type, float value);
        public void RemoveStatMultiplier(StatType type, float value);
        public void MultiplyStat(StatType type, float value);
        public void DivideStat(StatType type, float value);
        public void AddElementAttackMultiplier(Element element, float value);
        public void RemoveElementAttackMultiplier(Element element, float value);
        public void AddElementDamageRateMultiplier(Element element, float value);
        public void RemoveElementDamageRateMultiplier(Element element, float value);
        public void AddConditionResistance(ConditionTemplate condition, float value);
        public void RemoveConditionResistance(ConditionTemplate condition, float value);

        public bool IsFlagStat(FlagStatType type);
        public ReadOnlyReactiveProperty<bool> GetFlagProperty(FlagStatType type);
        public void AddFlagStat(FlagStatType type);
        public void RemoveFlagStat(FlagStatType type);

        public void AddWaitTime(float value);
        public void ResetWaitTime();
        public bool IsWaitTimeFull();
    }
}