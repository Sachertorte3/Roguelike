#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Utilities;

namespace Domain.Model
{
    public interface IGameManager
    {
        public UniTask<int> GetChoice(string? text, params string[] choices);
        public UniTask<string> GetTextInput();
        public void MoveMap(Location location, Id<IEntity> from);
    }
}