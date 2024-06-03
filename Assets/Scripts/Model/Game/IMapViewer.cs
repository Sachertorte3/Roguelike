#nullable enable
using Model.Domain.Map;

namespace Model.Game
{
    internal interface IMapViewer
    {
        public ITilemapViewer Tilemap { get; }
    }
}