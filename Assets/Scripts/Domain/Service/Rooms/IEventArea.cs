using Cysharp.Threading.Tasks;
using Domain.Service.Events;
using UnityEngine;

namespace Model.Game
{
    public interface IEventArea
    {
        public RectInt Rect { get; init; }
        public UniTask UpdatePosition(IGameManager gameManager, IMapManager mapManager, Vector2Int currentPosition);
    }
}