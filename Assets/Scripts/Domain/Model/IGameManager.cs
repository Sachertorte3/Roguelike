#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model
{
    public interface IGameManager
    {
        public UniTask<int> GetChoice(string? text, params string[] choices);
        public UniTask<string> GetTextInput();
        public void MoveMap(Id<IMap> destination, Id<IEntity> from);
        public void PlaySE(SE se);
    }
}