#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using ObservableCollections;
using UnityEngine;

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

        [SerializeField] private Vector2Int[] _grasses;
        public ObservableHashSet<Vector2Int> Grasses => new(_grasses);

        public TilemapMemento(int width, int height, IDictionary<Vector2Int, TileData> tiles,
            IEnumerable<Vector2Int> grasses)
        {
            var tileMementos = new TileMemento[width * height];
            foreach (var (position, tile) in tiles)
            {
                tileMementos[position.x + position.y * width] = tile.Serialize();
            }

            Width = width;
            _tiles = tileMementos;
            _grasses = grasses.ToArray();
        }

        public TilemapMemento(int width, TileMemento[] tiles, IEnumerable<Vector2Int> grasses)
        {
            Width = width;
            _tiles = tiles;
            _grasses = grasses.ToArray();
        }
    }
}