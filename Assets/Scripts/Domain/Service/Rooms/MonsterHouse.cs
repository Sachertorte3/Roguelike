using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Logs;
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
            return new RoomMemento
            {
                Room = rect,
                hasEntered = false,
                hasEverEntered = false
            };
        }

        public override RoomMemento Serialize()
        {
            return new RoomMemento
            {
                Room = Rect,
                hasEntered = hasEntered,
                hasEverEntered = hasEverEntered
            };
        }

        protected override void FirstTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            GameLog.Add("<color=red>モンスターハウスだ！</color>");
            for (var i = 0; i < 10; i++)
            {
                mapManager.SpawnRandomEnemy(mapManager.GetPassablePositionsInArea(Rect.RectRange()).GetAtRandom());
            }
        }
    }
}