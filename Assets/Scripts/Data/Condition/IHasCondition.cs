using Cysharp.Threading.Tasks;

namespace Data.Condition
{
    public interface IHasCondition
    {
        public UniTask<int> LoseHp(int value);
    }
}