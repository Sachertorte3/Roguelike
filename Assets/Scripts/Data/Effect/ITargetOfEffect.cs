using Cysharp.Threading.Tasks;
using Data.Condition;

namespace Data
{
    public interface ITargetOfEffect
    {
        public int MaxHp { get; }
        public UniTask GainHp(int value);
        public UniTask LoseHp(int value);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);
    }
}