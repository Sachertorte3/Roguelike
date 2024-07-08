using Domain.Model.Map;
using Domain.Service.Events;
using UnityEngine;

namespace Model.Game
{
    public abstract class Room<TMemento> : ISerializable<TMemento>, IEventArea
    {
        protected bool hasEntered = false;
        protected bool hasEverEntered = false;
        protected bool CanExecute = true;

        public Room(RoomMemento data)
        {
            Rect = data.Room;
            hasEntered = data.hasEntered;
            hasEverEntered = data.hasEverEntered;
        }

        public RectInt Rect { get; init; }

        public void UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition)
        {
            if (!CanExecute)
                return;

            var isInside = Rect.Contains(currentPosition);

            if (isInside)
            {
                UpdateTurnIfInside(gameManager, mapManager);
                if (!hasEntered)
                {
                    if (!hasEverEntered)
                    {
                        FirstTimeEnter(gameManager, mapManager);
                        hasEverEntered = true;
                    }

                    EveryTimeEnter(gameManager, mapManager);
                    hasEntered = true;
                }
            }
            else
            {
                UpdateTurnIfNotInside(gameManager, mapManager);
                if (hasEntered)
                {
                    EveryTimeExit(gameManager, mapManager);
                    hasEntered = false;
                }
            }
        }

        public abstract TMemento Serialize();

        protected virtual void UpdateTurnIfNotInside(IGameManager gameManager, IMapManager mapManager)
        {
        }

        protected virtual void UpdateTurnIfInside(IGameManager gameManager, IMapManager mapManager)
        {
        }

        protected virtual void FirstTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
        }

        protected virtual void EveryTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
        }

        protected virtual void EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
        }
    }
}