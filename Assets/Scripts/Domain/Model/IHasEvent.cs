#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IHasCharacterEvent
    {
        public ICharacterEvent Event { get; }
    }
    public interface IHasPlayerEvent
    {
        public IPlayerEvent Event { get; }
    }

    public interface ICharacterEvent
    {
        public bool CanExecuteEvent(ICharacter character);
        public UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map);
    }
    public interface IPlayerEvent
    {
        public bool CanExecuteEvent(IPlayer player);
        public UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map);
        public UniTask<IAction?> DoAction(IPlayer player, IGameManager gameManager, IMap map, IAction swap);
    }
}