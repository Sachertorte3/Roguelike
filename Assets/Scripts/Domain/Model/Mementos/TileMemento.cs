#nullable enable
using System;

namespace Domain.Model.Map
{
    [Serializable]
    public class TileMemento
    {
        public TileCategory TileType;
        public bool IsKnown;
    }
}