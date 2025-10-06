#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using ObservableCollections;
using R3;
using Utilities.Stats;

namespace Domain.Model.Character.Status
{
    public interface IStatusManager : IStats
    {
        public Observable<OnDamageReceivedMessage> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<ICondition> Conditions { get; }
        public UniTask UpdateTurn(IHasCondition hasCondition, bool enemyVisible);
        public IStat GetStat(StatType type);
        public IStat GetElementAttackMultiplierStat(Element element);
        public IStat GetElementDamageRateMultiplierStat(Element element);
        public IStat GetConditionResistanceStat(ConditionTemplate condition);
        public IFlagStat GetFlagStat(FlagStatType type);
        public void AddWaitTime(float value);
        public void ResetWaitTime();
        public bool IsWaitTimeFull();
    }
}