using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Domain.Model.Map;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using Unity.Logging;
using UnityEngine;
using Utilities;
using static RandomDungeonWithBluePrint.Constants;

namespace Domain.Service.Map
{
    public class Tilemap : IDisposable, ISerializable<TilemapMemento>, ITilemapViewer
    {
        private readonly HashSet<Vector2Int> _allPassablePositionsSet;
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesChanged = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesKnownChanged = new();
        private readonly ObservableDictionary<Vector2Int, TileData> _tiles;
        public readonly int Height;
        public readonly int Width;

        public Tilemap(TilemapMemento memento)
        {
            Width = memento.Tiles.GetLength(0);
            Height = memento.Tiles.GetLength(1);
            _tiles = new ObservableDictionary<Vector2Int, TileData>(Rect.RectRange()
                .ToDictionary(x => x, x => memento.Tiles[x.x, x.y]));

            _tiles.ObserveReplace()
                .Subscribe(context => _onTilesChanged.OnNext(new[] { (context.NewValue.Key, context.NewValue.Value) }));
            _allPassablePositionsSet = FindAllPassablePositions().ToHashSet();

            OnTilesChanged.Subscribe(changeTiles =>
            {
                foreach (var (position, tileData) in changeTiles)
                {
                    if (tileData.IsPassable())
                        _allPassablePositionsSet.Add(position);
                    else
                        _allPassablePositionsSet.Remove(position);
                    ResetMask(position);
                }
            });

            Rooms = new ReadOnlyCollection<RectInt>(memento.Rooms);
        }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(Rect.RectRange()
                .ToDictionary(x => x, _ => new TileData(TileCategory.Blank, false)));
            _tiles.ObserveReplace()
                .Subscribe(context => _onTilesChanged.OnNext(new[] { (context.NewValue.Key, context.NewValue.Value) }));
        }

        public ReadOnlyCollection<RectInt> Rooms { get; init; }

        public Vector2Int Size => new(Width, Height);

        public void Dispose()
        {
            _onTilesChanged.Dispose();
            _onTilesKnownChanged.Dispose();
        }

        public TilemapMemento Serialize()
        {
            var tiles = new TileData[Width, Height];
            foreach (var (position, tile) in _tiles)
            {
                tiles[position.x, position.y] = tile;
            }

            return new TilemapMemento(
                tiles,
                Rooms.ToList()
            );
        }

        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged => _onTilesChanged;
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesKnownChanged => _onTilesKnownChanged;
        public RectInt Rect => new(Vector2Int.zero, Size);

        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles()
        {
            return _tiles.Select(pair => (pair.Key, pair.Value));
        }

        public bool IsPassable(Vector2Int position)
        {
            return Get(position).IsPassable();
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            return new HashSet<Vector2Int>(_allPassablePositionsSet);
        }

        public static TilemapMemento Build(FieldBluePrint bluePrint)
        {
            var field = FieldBuilder.Build(bluePrint);
            var tiles = new TileData[field.Grid.Size.x, field.Grid.Size.y];
            var rooms = field.Rooms.Select(room => room.Rect);
            for (var x = 0; x < field.Grid.Size.x; x++)
            {
                for (var y = 0; y < field.Grid.Size.y; y++)
                {
                    var mapChipType = field.Grid[x, y];
                    var tileType = mapChipType == (int)MapChipType.Wall
                        ? TileCategory.Wall
                        : TileCategory.Floor;
                    tiles[x, y] = new TileData(tileType, false);
                }
            }

            return new TilemapMemento(tiles, rooms.ToList());
        }

        ~Tilemap()
        {
            Dispose();
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
                throw new ArgumentOutOfRangeException(
                    $"position {position} is out of map (MapSize Width:{Width}, Height:{Height})");
            }

            return _tiles[position];
        }

        private IEnumerable<Vector2Int> FindAllPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsPassable()).Select(pair => pair.position);
        }

        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown)
        {
            var changedPositions = positions.Select(position => (position, Get(position))).Where(pair => pair.Item2.IsKnown != isKnown);
            foreach (var (_, tile) in changedPositions)
            {
                tile.SetKnown(isKnown);
            }
            _onTilesKnownChanged.OnNext(changedPositions);
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            var changedPositions = positions.Where(position => Get(position).TileType == TileCategory.Wall);
            foreach (var position in changedPositions)
            {
                _tiles[position] = new TileData(TileCategory.Floor, false);
            }
            _onTilesChanged.OnNext(changedPositions.Select(position => (position, Get(position))));
        }

        public void ResetMask(Vector2Int position)
        {
            SetTilesKnown(new RectInt(position - new Vector2Int(1, 1), new Vector2Int(3, 3)).RectRange(), false);
        }
    }
}