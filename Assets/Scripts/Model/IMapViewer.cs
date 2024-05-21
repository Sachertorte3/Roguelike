#nullable enable
using Model.Map;

namespace Model
{
    internal interface IMapViewer
    {
        public ITilemapViewer Tilemap { get; }
    }
}