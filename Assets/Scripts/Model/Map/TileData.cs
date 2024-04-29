using System.ComponentModel;

namespace Scripts.Model.Map
{
    public record TileData(TileCategory TileType)
    {
        public bool IsPassable()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Wall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException(),
            };
        }
    }
}
