#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TilemapMemento
    {
        [field: SerializeField] public int Width { get; private set; }
        [field: SerializeField] public TileMemento[] Tiles { get; private set; }
        [field: SerializeField] public RectInt[] Rooms { get; private set; }
        public TilemapMemento(int width, TileMemento[] tiles, RectInt[] rooms)
        {
            Width = width;
            Tiles = tiles;
            Rooms = rooms;
        }
    }
}