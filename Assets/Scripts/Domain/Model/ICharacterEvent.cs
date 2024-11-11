using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface ICharacterEvent
    {
        public bool CanExecuteEvent(ICharacter character);
        public UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map);
    }
}