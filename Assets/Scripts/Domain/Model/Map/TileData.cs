using System.ComponentModel;
using Domain.Model.Memento;

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
            (
                tileType: TileType,
                isKnown: IsKnown
            );
        }

        public static TileMemento Build(TileCategory tileType, bool isKnown)
        {
            return new TileMemento
            (
                tileType: tileType,
                isKnown: isKnown
            );
        }

        public bool IsWalkable()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => false,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public bool IsPassable()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public bool IsTransparent()
        {
            return TileType switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
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