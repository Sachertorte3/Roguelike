using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
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
        private TilemapMemento _mementoCache;
        public readonly int Height;
        public readonly int Width;

        public Tilemap(TilemapMemento memento)
        {
            Width = memento.Width;
            Height = memento.Tiles.Length / Width;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(memento.Tiles.ToList().Select((x, index) => (new Vector2Int(index % Width, index / Width), new TileData(x)))
                .ToDictionary(x => x.Item1, x => x.Item2));

            Rooms = new(memento.Rooms.Select(room => new RectInt(room.x, room.y, room.width, room.height)).ToList());

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
                UpdateMementoCache();
            });
            OnTilesKnownChanged.Subscribe(changeTiles =>
            {
                UpdateMementoCache();
            });
            UpdateMementoCache();
        }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(Rect.RectRange()
                .ToDictionary(x => x, _ => new TileData(TileData.Build(TileCategory.Blank, false))));
        }

        public ReadOnlyCollection<RectInt> Rooms { get; init; }

        public Vector2Int Size => new(Width, Height);

        public void Dispose()
        {
            _onTilesChanged.Dispose();
            _onTilesKnownChanged.Dispose();
        }

        private void UpdateMementoCache()
        {
            var tiles = new TileMemento[Width * Height];
            foreach (var (position, tile) in _tiles)
            {
                tiles[position.x + (position.y * Width)] = tile.Serialize();
            }

            _mementoCache = new TilemapMemento
            {
                Width = Width,
                Tiles = tiles,
                Rooms = Rooms.ToArray()
            };
        }

        public TilemapMemento Serialize()
        {
            return _mementoCache;
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
            var width = field.Grid.Size.x + 2;
            var height = field.Grid.Size.y + 2;
            var tiles = new TileMemento[width * height];
            var rooms = field.Rooms.Select(room => room.Rect).Select(rect => new RectInt(rect.position + new Vector2Int(1, 1), rect.size));

            for (var x = -1; x < field.Grid.Size.x + 1; x++)
            {
                for (var y = -1; y < field.Grid.Size.y + 1; y++)
                {
                    TileCategory tileType;
                    if (x == -1 || y == -1 || x == field.Grid.Size.x || y == field.Grid.Size.y)
                    {
                        tileType = TileCategory.UnbreakableWall;
                    }
                    else
                    {
                        var mapChipType = field.Grid[x, y];
                        tileType = mapChipType == (int)MapChipType.Wall
                            ? TileCategory.Wall
                            : TileCategory.Floor;
                    }
                    tiles[x + 1 + ((y + 1) * width)] = TileData.Build(tileType, false);
                }
            }

            return new TilemapMemento
            {
                Width = width,
                Tiles = tiles,
                Rooms = rooms.ToArray()
            };
        }

        ~Tilemap()
        {
            Dispose();
        }

        public bool IsPositionInsideMap(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }

        public TileData Get(Vector2Int position)
        {
            if (!IsPositionInsideMap(position))
            {
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
            var changedPositions = positions.Select(position => (position, Get(position))).Where(pair => pair.Item2.IsKnown != isKnown).ToList();
            foreach (var (_, tile) in changedPositions)
            {
                tile.SetKnown(isKnown);
            }
            _onTilesKnownChanged.OnNext(changedPositions);
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            var changedPositions = positions.Where(position => Get(position).TileType == TileCategory.Wall).ToList();
            foreach (var position in changedPositions)
            {
                _tiles[position] = new TileData(TileData.Build(TileCategory.Floor, false));
            }
            _onTilesChanged.OnNext(changedPositions.Select(position => (position, Get(position))));
        }

        public void ResetMask(Vector2Int position)
        {
            SetTilesKnown(new RectInt(position - new Vector2Int(1, 1), new Vector2Int(3, 3)).RectRange(), false);
        }
    }
}