using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class MonsterHouse : Room<RoomMemento>
    {
        public MonsterHouse(RoomMemento data, Vector2Int playerPosition) : base(data, playerPosition)
        {
        }

        public static RoomMemento Build(RectInt rect)
        {
            return new RoomMemento
            (
                rect,
                false,
                false
            );
        }

        public override RoomMemento Serialize()
        {
            return new RoomMemento
            (
                Rect,
                hasEntered,
                hasEverEntered
            );
        }

        protected override async UniTask FirstTimeEnter(IGameManager gameManager, IMap mapManager)
        {
            GameLog.Add("<color=red>モンスターハウスだ！</color>");
            for (var i = 0; i < 10; i++)
            {
                mapManager.SpawnRandomEnemy(mapManager.GetBlankAndStandablePositionsInArea(Rect.RectRange())
                    .GetAtRandom(), false, false);
            }

            await UniTask.Delay(1000);
        }
    }
}