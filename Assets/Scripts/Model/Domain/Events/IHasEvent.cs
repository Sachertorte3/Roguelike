namespace Model.Domain.Events
{
    public interface IHasEvent
    {
        public void DoEvent(IGameManager gameManager, IMapManager mapManager);
    }
}