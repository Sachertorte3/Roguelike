using Domain.Service.Events;
using UnityEngine;

namespace Model.Game
{
    public interface IEventArea
    {
        public RectInt Rect { get; init; }
        public void UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition);
    }
}