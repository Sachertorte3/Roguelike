namespace Domain.Service.Events
{
    public interface IHasEvent
    {
        public bool CanExecuteEvent { get; }
        public void DoEvent(IGameManager gameManager, IMapManager mapManager);
    }
}