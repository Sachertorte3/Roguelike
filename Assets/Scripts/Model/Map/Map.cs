using RandomDungeonWithBluePrint;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Logging;
using UnityEngine;
using static RandomDungeonWithBluePrint.Constants;

namespace Scripts.Model.Map
{
    public class Map
    {
        public IObservable<(Vector2Int position, TileData tile)> OnChangeTile => _onChangeTile;
        private readonly Subject<(Vector2Int, TileData)> _onChangeTile = new Subject<(Vector2Int, TileData)>();
        private readonly ReactiveDictionary<Vector2Int, TileData> _tiles;
        public readonly int Width;
        public readonly int Height;
        public Map(FieldBluePrint bluePrint)
        {
            Field field = FieldBuilder.Build(bluePrint);
            Width = field.Grid.Size.x;
            Height = field.Grid.Size.y;
            _tiles = new ReactiveDictionary<Vector2Int, TileData>(
                new RectInt(0, 0, Width, Height)
                    .RectRange().ToDictionary(
                        position => position,
                        position =>
                        {
                            int mapChipType = field.Grid[position.x, position.y];
                            TileCategory tileType = mapChipType == (int)MapChipType.Wall ? TileCategory.Wall : TileCategory.Floor;
                            return new TileData(tileType);
                        }
                    )
            );
            _tiles.ObserveReplace().Subscribe(context => _onChangeTile.OnNext((context.Key, context.NewValue)));
        }
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ReactiveDictionary<Vector2Int, TileData>(new RectInt(0, 0, Width, Height).RectRange().ToDictionary(x => x, _ => new TileData(TileCategory.Blank)));
            _tiles.ObserveReplace().Subscribe(context => _onChangeTile.OnNext((context.Key, context.NewValue)));
        }
        public bool IsPositionInsideMap(Vector2Int position)
        {
            return position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height;
        }
        public TileData Get(Vector2Int position)
        {
            if (IsPositionInsideMap(position))
            {
                Log.Fatal($"position {position} is out of map (MapSize Width:{Width}, Height:{Height})");
                throw new ArgumentOutOfRangeException($"position {position} is out of map (MapSize Width:{Width}, Height:{Height})");
            }
            return _tiles[position];
        }
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles()
        {
            return _tiles.Select(pair => (pair.Key, pair.Value));
        }
        public bool IsPassable(Vector2Int position)
        {
            return Get(position).IsPassable();
        }
        public IEnumerable<Vector2Int> GetAllPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsPassable()).Select(pair => pair.position);
        }
    }
}
