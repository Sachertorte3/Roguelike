#nullable enable
using System;
using Domain.Model.Map;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TileMemento
    {
        public TileCategory TileType;
        public bool IsKnown;
    }
}