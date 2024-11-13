#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IPlayerEvent
    {
        public bool CanExecuteEvent(IPlayer player);
        public UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map);
        public UniTask<IAction?> DoAction(IPlayer player, IGameManager gameManager, IMap map, IAction? swap);
    }
}