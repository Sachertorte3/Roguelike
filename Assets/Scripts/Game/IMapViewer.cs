#nullable enable
using Domain.Service.Map;
using UnityEngine;

namespace Model.Game
{
    internal interface IMapViewer
    {
        public ITilemapViewer TilemapViewer { get; }
        public RectInt? ShopRect { get; }
    }
}