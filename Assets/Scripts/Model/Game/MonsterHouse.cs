using Data.Map;
using Model.Domain.Events;
using UnityEngine;
using Utilities;

namespace Model.Game
{
    public class MonsterHouse : ISerializable<MonsterHouseMemento>, IEventArea
    {
        public RectInt Rect { get; init; }
        private bool hasEntered = false;
        private bool hasEverEntered = false;

        public MonsterHouse(MonsterHouseMemento data)
        {
            Rect = data.Room;
            hasEntered = data.hasEntered;
            hasEverEntered = data.hasEverEntered;
        }
        public static MonsterHouseMemento Build(RectInt rect)
        {
            return new MonsterHouseMemento(rect, false, false);
        }
        public MonsterHouseMemento Serialize()
        {
            return new MonsterHouseMemento(Rect, hasEntered, hasEverEntered);
        }

        public void UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition)
        {
            bool isInside = Rect.Contains(currentPosition);

            if (isInside && !hasEntered)
            {
                if (!hasEverEntered)
                {
                    FirstTimeEnter(gameManager, mapManager);
                    hasEverEntered = true;
                }
                EveryTimeEnter(gameManager, mapManager);
                hasEntered = true;
            }
            else if (!isInside && hasEntered)
            {
                EveryTimeExit(gameManager, mapManager);
                hasEntered = false;
            }
        }

        private void FirstTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            Debug.Log("First time entering the Monster House.");
            for (int i = 0; i < 10; i++)
            {
                mapManager.SpawnRandomEnemy(Rect.RectRange().GetAtRandom());
            }
        }

        private void EveryTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            Debug.Log("Entering the Monster House.");
        }

        private void EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
            Debug.Log("Exiting the Monster House.");
        }
    }
}