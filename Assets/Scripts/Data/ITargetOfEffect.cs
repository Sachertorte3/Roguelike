using Cysharp.Threading.Tasks;

namespace Data
{
    public interface ITargetOfEffect
    {
        public int MaxHp { get; }
        public UniTask GainHp(int value);
        public UniTask LoseHp(int value);
    }
}