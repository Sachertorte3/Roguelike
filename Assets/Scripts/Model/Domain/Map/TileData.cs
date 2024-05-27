using System.ComponentModel;

namespace Model.Domain.Map
{
    public record TileData
    {
        public TileCategory TileType { get; }
        public bool IsKnown { get; private set; }
        public TileData(TileCategory TileType, bool IsKnown)
        {
            this.TileType = TileType;
            this.IsKnown = IsKnown;
        }
        public bool IsPassable()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Wall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }
        public void SetKnown(bool isKnown)
        {
            IsKnown = isKnown;
        }
    }
}

