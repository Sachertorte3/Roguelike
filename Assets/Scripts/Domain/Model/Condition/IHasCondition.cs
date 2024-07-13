using Cysharp.Threading.Tasks;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
        public void AddMaxHpValue(float value);
        public void AddMaxHpMultiplier(float value);
        public void AddViewRangeMultiplier(float value);
        public void RemoveMaxHpValue(float value);
        public void RemoveMaxHpMultiplier(float value);
        public void RemoveViewRangeMultiplier(float value);
        public void AddClairvoyantFlags();
        public void RemoveClairvoyantFlags();
        public UniTask<int> LoseHp(int value);
    }
}