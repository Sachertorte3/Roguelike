using Domain.Model.Character;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
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
        public void AddOverDriveFlags();
        public void RemoveOverDriveFlags();
        public int LoseHp(float value, bool notifyOnlyActualLoss = false);
        public void AddWaitTime(float value);
        public void ResetWaitTime();
        public bool IsWaitTimeFull();
    }
    public enum StatType
    {
        MaxHp,
        HpNaturalRecovery,
        ViewRange,
        WaitTime
    }
}