using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    public interface IActor
    {
        public bool CanMove(Direction8 direction);
        public UniTask Move(Direction8 direction);
    }
}