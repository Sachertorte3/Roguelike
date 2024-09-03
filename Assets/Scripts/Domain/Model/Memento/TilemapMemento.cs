#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TilemapMemento
    {
        public int Width;
        public TileMemento[] Tiles;
        public RectInt[] Rooms;
    }
}