using Cysharp.Threading.Tasks;

namespace Data.Condition
{
    public interface IHasCondition
    {
        public void AddMaxHpValue(float value);
        public void AddMaxHpMultiplier(float value);
        public void RemoveMaxHpValue(float value);
        public void RemoveMaxHpMultiplier(float value);
        public UniTask<int> LoseHp(int value);
    }
}