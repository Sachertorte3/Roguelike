using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Service.Rooms
{
    public interface IEventArea
    {
        public RectInt Rect { get; init; }
        public UniTask UpdatePosition(IGameManager gameManager, IMap mapManager, Vector2Int currentPosition);
    }
}