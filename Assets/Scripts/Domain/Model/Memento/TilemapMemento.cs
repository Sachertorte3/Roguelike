#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using ObservableCollections;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TilemapMemento
    {
        [field: SerializeField] public int Width { get; private set; }
        public int Height => _tiles.Length / Width;
        [SerializeField] private TileMemento[] _tiles;

        public ObservableDictionary<Vector2Int, TileData> Tiles => new(
            _tiles
                .Select((x, index) => (new Vector2Int(index % Width, index / Width), new TileData(x)))
                .ToDictionary(x => x.Item1, x => x.Item2));

        [SerializeField] private SerializableDictionary<Vector2Int, OverlayTileCategory> _overlayTiles;
        public ObservableDictionary<Vector2Int, OverlayTileCategory> OverlayTiles => new(_overlayTiles);

        public TilemapMemento(int width, int height, IDictionary<Vector2Int, TileData> tiles,
            IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            var tileMementos = new TileMemento[width * height];
            foreach (var (position, tile) in tiles)
            {
                tileMementos[position.x + position.y * width] = tile.Serialize();
            }

            Width = width;
            _tiles = tileMementos;
            _overlayTiles = overlayTiles.ToSerializable();
        }

        public TilemapMemento(int width, TileMemento[] tiles, IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            Width = width;
            _tiles = tiles;
            _overlayTiles = overlayTiles.ToSerializable();
        }
    }
}