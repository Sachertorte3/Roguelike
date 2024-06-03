using Cysharp.Threading.Tasks;

namespace Data.Condition
{
    public interface IHasCondition
    {
        public UniTask LoseHp(int value);
    }
}