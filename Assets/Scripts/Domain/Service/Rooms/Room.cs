using Domain.Model.Memento;
using Domain.Service.Events;
using R3;
using UnityEngine;

namespace Model.Game
{
    public abstract class Room<TMemento> : ISerializable<TMemento>, IEventArea
    {
        protected bool hasEntered = false;
        protected bool hasEverEntered = false;
        public bool CanExecute { get; protected set; } = true;
        private ReactiveProperty<bool> _isInside;
        public ReadOnlyReactiveProperty<bool> IsInside => _isInside;

        public Room(RoomMemento data, Vector2Int playerPosition)
        {
            Rect = data.Room;
            _isInside = new(Rect.Contains(playerPosition));
            hasEntered = data.hasEntered;
            hasEverEntered = data.hasEverEntered;
        }

        public RectInt Rect { get; init; }

        public void UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition)
        {
            if (!CanExecute)
                return;

            _isInside.Value = Rect.Contains(currentPosition);
            if (_isInside.Value)
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