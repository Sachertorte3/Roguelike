#nullable enable
using Domain.Service.Map;

namespace Model.Game
{
    internal interface IMapViewer
    {
        public ITilemapViewer Tilemap { get; }
    }
}