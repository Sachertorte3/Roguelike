#nullable enable
using Domain.Model.Condition;
using ObservableCollections;
using R3;

namespace Domain.Model.Character
{
    public interface IStatusManager
    {
        public IStats Stats { get; }
        public bool IsDead { get; }
        public bool IsOverDrive { get; }
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
        public void AddClairvoyantFlags();
        public void RemoveClairvoyantFlags();
        public void AddBlindFlags();
        public void RemoveBlindFlags();
        public void AddOverDriveFlags();
        public void RemoveOverDriveFlags();
        public int LoseHp(float value, bool notifyOnlyActualLoss = false);
        public void AddWaitTime(float value);
        public void ResetWaitTime();
        public bool IsWaitTimeFull();
    }
}