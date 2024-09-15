using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;

namespace Domain.Service.Rooms
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

        public async UniTask UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition)
        {
            if (!CanExecute)
                return;

            _isInside.Value = Rect.Contains(currentPosition);
            if (_isInside.Value)
            {
                await UpdateTurnIfInside(gameManager, mapManager);
                if (!hasEntered)
                {
                    if (!hasEverEntered)
                    {
                        await FirstTimeEnter(gameManager, mapManager);
                        hasEverEntered = true;
                    }

                    await EveryTimeEnter(gameManager, mapManager);
                    hasEntered = true;
                }
            }
            else
            {
                await UpdateTurnIfNotInside(gameManager, mapManager);
                if (hasEntered)
                {
                    await EveryTimeExit(gameManager, mapManager);
                    hasEntered = false;
                }
            }
        }

        public abstract TMemento Serialize();

        protected virtual UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMapManager mapManager)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask UpdateTurnIfInside(IGameManager gameManager, IMapManager mapManager)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask FirstTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask EveryTimeEnter(IGameManager gameManager, IMapManager mapManager)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask EveryTimeExit(IGameManager gameManager, IMapManager mapManager)
        {
            return UniTask.CompletedTask;
        }
    }
}