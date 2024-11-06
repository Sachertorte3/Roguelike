#nullable enable
using Domain.Model.Condition;
using ObservableCollections;
using R3;

namespace Domain.Model.Character
{
    public interface IStatusManager
    {
        public IStats Stats { get; }
        public bool CannotAct { get; }
        public bool CannotMove { get; }
        public bool IsOverDrive { get; }
        public bool IsConfused { get; }
        public bool IsHard { get; }
        public bool IsHeavy { get; }
        public bool IsSecureHold { get; }
        public bool IsCurseProof { get; }
        public ReadOnlyReactiveProperty<bool> IsAffectedByTraps { get; }
        public Observable<int> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<ICondition> Conditions { get; }
        public void UpdateTurn(IHasCondition hasCondition, bool enemyVisible);
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
        public void AddFlagStat(FlagStatType type);
        public void RemoveFlagStat(FlagStatType type);
        public int LoseHp(float value, bool notifyOnlyActualLoss = false);
        public void AddWaitTime(float value);
        public void ResetWaitTime();
        public bool IsWaitTimeFull();
    }
}