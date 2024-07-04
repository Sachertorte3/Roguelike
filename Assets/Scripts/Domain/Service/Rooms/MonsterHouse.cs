using Domain.Model.Map;
using Domain.Service.Events;
using UnityEngine;
using Utilities;

namespace Model.Game
{
    public class MonsterHouse : Room<RoomMemento>
    {
        public MonsterHouse(RoomMemento data) : base(data)
        {
        }

        public static RoomMemento Build(RectInt rect)
        {
            return new RoomMemento(rect, false, false);
        }

        public override RoomMemento Serialize()
        {
            return new RoomMemento(Rect, hasEntered, hasEverEntered);
        }

        protected override void FirstTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            Debug.Log("First time entering the Monster House.");
            for (var i = 0; i < 10; i++)
            {
                mapManager.SpawnRandomEnemy(Rect.RectRange().GetAtRandom());
            }
        }
    }
}