using System.ComponentModel;

namespace Domain.Model.Map
{
    public class TileData : ISerializable<TileMemento>
    {
        public readonly TileCategory TileType;
        public bool IsKnown { get; private set; }

        public TileData(TileMemento memento)
        {
            TileType = memento.TileType;
            IsKnown = memento.IsKnown;
        }

        public TileMemento Serialize()
        {
            return new TileMemento
            {
                TileType = TileType,
                IsKnown = IsKnown
            };
        }
         
        public static TileMemento Build(TileCategory tileType, bool isKnown)
        {
            return new TileMemento
            {
                TileType = tileType,
                IsKnown = isKnown
            };
        }

        public bool IsPassable()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
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