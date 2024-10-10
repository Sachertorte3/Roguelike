using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Map
{
    public class Tilemap : IDisposable, ISerializable<TilemapMemento>, ITilemapViewer
    {
        private readonly HashSet<Vector2Int> _allWalkablePositionsSet;
        private readonly HashSet<Vector2Int> _allPassablePositionsSet;
        private readonly HashSet<Vector2Int> _allLightPassablePositionsSet;
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesChanged = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, bool IsGrass)>> _onGrassesChanged = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, bool IsKnown)>> _onTilesKnownChanged = new();
        private readonly ObservableDictionary<Vector2Int, TileData> _tiles;
        private readonly ObservableHashSet<Vector2Int> _grasses;
        private TilemapMemento _mementoCache;
        public readonly int Height;
        public readonly int Width;

        public Tilemap(TilemapMemento memento)
        {
            Width = memento.Width;
            Height = memento.Height;
            _tiles = memento.Tiles;
            _grasses = new ObservableHashSet<Vector2Int>(memento.Grasses);

            _allWalkablePositionsSet = FindAllWalkablePositions().ToHashSet();
            _allPassablePositionsSet = FindAllPassablePositions().ToHashSet();
            _allLightPassablePositionsSet = FindAllLightPassablePositions().ToHashSet();

            OnTilesChanged.Subscribe(changeTiles =>
            {
                foreach (var (position, tileData) in changeTiles)
                {
                    if (tileData.IsWalkable())
                        _allWalkablePositionsSet.Add(position);
                    else
                        _allWalkablePositionsSet.Remove(position);

                    if (tileData.IsPassable())
                        _allPassablePositionsSet.Add(position);
                    else
                        _allPassablePositionsSet.Remove(position);

                    if (tileData.IsTransparent())
                        _allLightPassablePositionsSet.Add(position);
                    else
                        _allLightPassablePositionsSet.Remove(position);

                    ResetMask(position);
                }

                UpdateMementoCache();
            });
            OnGrassesChanged.Subscribe(changeGrasses => { UpdateMementoCache(); });
            OnTilesKnownChanged.Subscribe(changeTiles => { UpdateMementoCache(); });
            UpdateMementoCache();
        }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(Rect.RectRange()
                .ToDictionary(x => x, _ => new TileData(TileData.Build(TileCategory.Blank, false))));
        }

        public Vector2Int Size => new(Width, Height);

        public void Dispose()
        {
            _onTilesChanged.Dispose();
            _onTilesKnownChanged.Dispose();
        }

        private void UpdateMementoCache()
        {
            _mementoCache = new TilemapMemento
            (
                Width,
                Height,
                _tiles,
                _grasses
            );
        }

        public void UpdateTurn()
        {
            var grasses = new List<Vector2Int>();
            foreach (var position in _grasses)
            {
                if (Random.value < 1 / 256f)
                {
                    var spawnPosition = position + DirectionMethods.AllDirections.GetAtRandom().Vector();
                    grasses.Add(spawnPosition);
                }
            }

            SetGrasses(grasses, true);
        }

        public TilemapMemento Serialize()
        {
            return _mementoCache;
        }

        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged => _onTilesChanged;
        public Observable<IEnumerable<(Vector2Int Position, bool IsGrass)>> OnGrassesChanged => _onGrassesChanged;
        public Observable<IEnumerable<(Vector2Int Position, bool IsKnown)>> OnTilesKnownChanged => _onTilesKnownChanged;
        public RectInt Rect => new(Vector2Int.zero, Size);

        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles()
        {
            return _tiles.Select(pair => (pair.Key, pair.Value));
        }

        public IEnumerable<Vector2Int> GetAllGrasses()
        {
            return _grasses.ToArray();
        }

        public bool IsWalkable(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsWalkable());
        }

        public bool IsPassable(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsPassable());
        }

        public bool IsTransparent(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsTransparent());
        }

        public HashSet<Vector2Int> GetAllWalkablePositions()
        {
            return new HashSet<Vector2Int>(_allWalkablePositionsSet);
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            return new HashSet<Vector2Int>(_allPassablePositionsSet);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return new HashSet<Vector2Int>(_allLightPassablePositionsSet);
        }

        ~Tilemap()
        {
            Dispose();
        }

        public bool IsPositionInsideMap(Vector2Int position)
        {
            return _tiles.ContainsKey(position);
        }

        public Option<TileData> GetTile(Vector2Int position)
        {
            if (!IsPositionInsideMap(position))
            {
                Log.Info($"position {position} is out of map (MapSize Width:{Width}, Height:{Height})");
                return Option<TileData>.None;
            }

            return Option.Some(_tiles[position]);
        }

        private IEnumerable<Vector2Int> FindAllWalkablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsWalkable()).Select(pair => pair.position);
        }

        private IEnumerable<Vector2Int> FindAllPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsPassable()).Select(pair => pair.position);
        }

        private IEnumerable<Vector2Int> FindAllLightPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsTransparent()).Select(pair => pair.position);
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            var result = new List<(Vector2Int position, bool isGrass)>();
            foreach (var position in positions)
            {
                if (isGrass != _grasses.Contains(position))
                {
                    if (isGrass)
                    {
                        if (GetTile(position).MapOr(false, tile => tile.TileType == TileCategory.Floor))
                        {
                            _grasses.Add(position);
                            result.Add((position, true));
                        }
                    }
                    else
                    {
                        _grasses.Remove(position);
                        result.Add((position, false));
                    }
                }
            }

            _onGrassesChanged.OnNext(result);
        }

        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown)
        {
            var changedPositions = positions
                .Select(position => (position, GetTile(position)))
                .Where(pair => pair.Item2.MapOr(false, tile => tile.IsKnown != isKnown))
                .Select(pair => (pair.position, pair.Item2.Expect("tile is null")));
            var result = new List<(Vector2Int position, bool isKnown)>();
            foreach (var (position, tile) in changedPositions)
            {
                tile.SetKnown(isKnown);
                result.Add((position, isKnown));
            }

            _onTilesKnownChanged.OnNext(result);
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            var changedPositions = positions
                .Select(position => (position, GetTile(position)))
                .Where(pair => pair.Item2.MapOr(false, tile => tile.TileType == TileCategory.Wall))
                .Select(pair => (pair.position, pair.Item2.Expect("tile is null")));
            var result = new List<(Vector2Int position, TileData tileData)>();
            foreach (var (position, tile) in changedPositions)
            {
                _tiles[position] = new TileData(TileData.Build(TileCategory.Floor, false));
                result.Add((position, _tiles[position]));
            }

            _onTilesChanged.OnNext(result);
        }

        public void ResetMask(Vector2Int position)
        {
            SetTilesKnown(new RectInt(position - new Vector2Int(1, 1), new Vector2Int(3, 3)).RectRange(), false);
        }
    }
}