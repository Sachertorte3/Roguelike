using RandomDungeonWithBluePrint;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Unity.Logging;
using UnityEngine;
using UnityEngine.Tilemaps;
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
                            TileType tileType = mapChipType == (int)MapChipType.Wall ? TileType.Wall : TileType.Floor;
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
            _tiles = new ReactiveDictionary<Vector2Int, TileData>(new RectInt(0, 0, Width, Height).RectRange().ToDictionary(x => x, _ => new TileData(TileType.Blank)));
            _tiles.ObserveReplace().Subscribe(context => _onChangeTile.OnNext((context.Key, context.NewValue)));
        }
        public TileData Get(Vector2Int position)
        {
            if (position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height)
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
    }
    public record TileData(TileType TileType);
    public enum TileType
    {
        Floor,
        Wall,
        Blank,
    }
}
