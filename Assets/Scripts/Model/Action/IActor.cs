using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    public interface IActor
    {
        public UniTask Move(Direction8 direction);
    }
}