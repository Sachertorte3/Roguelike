namespace Domain.Service.Events
{
    public interface IHasEvent
    {
        public void DoEvent(IGameManager gameManager, IMapManager mapManager);
    }
}