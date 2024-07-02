using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Characters;
using Domain.Service.Events;
using Model.Game;
using R3;

namespace Domain.Service.Rooms
{
    public class Clerk : IHasEvent
    {
        public readonly Character Character;
        public readonly Shop _shop;
        private readonly Subject<Unit> _onEventDone = new();
        public Observable<Unit> OnEventDone => _onEventDone;
        public Clerk(Character character, Shop shop)
        {
            Character = character;
            _shop = shop;
        }
        public void DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            _onEventDone.OnNext(Unit.Default);
        }
    }
}