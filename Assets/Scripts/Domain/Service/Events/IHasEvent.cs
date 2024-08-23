using Cysharp.Threading.Tasks;

namespace Domain.Service.Events
{
    public interface IHasEvent
    {
        public bool CanExecuteEvent { get; }
        public UniTask DoEvent(IGameManager gameManager, IMapManager mapManager);
    }
}