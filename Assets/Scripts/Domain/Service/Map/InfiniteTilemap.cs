using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;
using Utilities.WorldCreater;
using TileData = Domain.Model.Map.TileData;

namespace Domain.Service.Map
{
    public class InfiniteTilemap : IDisposable, ISerializable<TilemapMemento>, ITilemap
    {
        public const int CHUNK_SIZE = 16;
        public const int ACTIVE_CHUNK_SIZE = 4;
        private readonly WorldGenerator _worldCreater;
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesChanged = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesLoaded = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>> _onOverlayTilesChanged = new();
        private readonly Subject<IEnumerable<(Vector2Int Position, bool IsKnown)>> _onTilesKnownChanged = new();
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged => _onTilesChanged;
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesLoaded => _onTilesLoaded;
        public Observable<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>> OnOverlayTilesChanged => _onOverlayTilesChanged;
        public Observable<IEnumerable<(Vector2Int Position, bool IsKnown)>> OnTilesKnownChanged => _onTilesKnownChanged;

        private readonly ObservableDictionary<Vector2Int, TileData> _tiles;
        private readonly ObservableDictionary<Vector2Int, OverlayTileCategory> _overlayTiles;
        private readonly ReactiveProperty<Vector2Int> _existingChunk = new();
        public ReadOnlyReactiveProperty<RectInt> Rect => _existingChunk.Select(chunk => ToMapRect(chunk)).ToReadOnlyReactiveProperty();
        public InfiniteTilemap(TilemapMemento memento)
        {
            _worldCreater = new WorldGenerator(0);
            _tiles = memento.Tiles;
            _overlayTiles = memento.OverlayTiles;
        }

        public void Dispose()
        {
            _onTilesChanged.Dispose();
            _onOverlayTilesChanged.Dispose();
            _onTilesKnownChanged.Dispose();
        }

        public IEnumerable<Vector2Int> GetAllGrasses()
        {
            return _overlayTiles.Where(pair => pair.Value == OverlayTileCategory.Grass).Select(pair => pair.Key);
        }

        public IEnumerable<Vector2Int> GetAllIces()
        {
            return _overlayTiles.Where(pair => pair.Value == OverlayTileCategory.FloatingIce).Select(pair => pair.Key);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return _tiles.Where(pair => pair.Value.IsTransparent()).Select(pair => pair.Key).ToHashSet();
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            return _tiles.Where(pair => pair.Value.IsPassable()).Select(pair => pair.Key).ToHashSet();
        }

        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles()
        {
            return _tiles.Select(pair => (pair.Key, pair.Value));
        }

        public HashSet<Vector2Int> GetAllWalkablePositions()
        {
            return _tiles.Where(pair => pair.Value.IsWalkable()).Select(pair => pair.Key).ToHashSet();
        }

        public Option<TileData> GetTile(Vector2Int position)
        {
            if (!_tiles.ContainsKey(position))
            {
                LoadTiles(new[] { position });
            }
            return Option.Some(_tiles[position]);
        }

        public bool IsGrass(Vector2Int position)
        {
            return _overlayTiles.ContainsKey(position) && _overlayTiles[position] == OverlayTileCategory.Grass;
        }

        public bool IsIce(Vector2Int position)
        {
            return _overlayTiles.ContainsKey(position) && _overlayTiles[position] == OverlayTileCategory.FloatingIce;
        }

        public bool IsPassable(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsPassable());
        }

        public bool IsTransparent(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsTransparent());
        }

        public bool IsWalkable(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsWalkable());
        }

        public TilemapMemento Serialize()
        {
            return new TilemapMemento(_tiles, _overlayTiles);
        }

        private Vector2Int ToChunk(Vector2Int position)
        {
            return new Vector2Int(
                position.x < 0 ? (position.x + 1) / CHUNK_SIZE - 1 : position.x / CHUNK_SIZE,
                position.y < 0 ? (position.y + 1) / CHUNK_SIZE - 1 : position.y / CHUNK_SIZE
            );
        }

        private RectInt ToChunkRect(Vector2Int chunk)
        {
            return new RectInt(chunk * CHUNK_SIZE, Vector2Int.one * CHUNK_SIZE);
        }

        private RectInt ToMapRect(Vector2Int chunk)
        {
            return new RectInt((chunk - Vector2Int.one * 2) * CHUNK_SIZE, Vector2Int.one * CHUNK_SIZE * 5);
        }

        public void UpdateChunk(Vector2Int position)
        {
            var newChunk = ToChunk(position);
            if (_existingChunk.Value != newChunk)
            {
                _existingChunk.Value = newChunk;
                foreach (var chunk in EnumerableExtension.CircleRange(newChunk, ACTIVE_CHUNK_SIZE))
                {
                    LoadChunk(chunk);
                }
            }
        }

        private void LoadChunk(Vector2Int chunk)
        {
            LoadTiles(ToChunkRect(chunk).RectRange());
        }

        private void LoadTiles(IEnumerable<Vector2Int> positions)
        {
            var result = new List<(Vector2Int position, TileData tileData)>();
            foreach (var position in positions)
            {
                if (_tiles.ContainsKey(position))
                    continue;
                var tile = _worldCreater.GetTile(position);
                var tileType = tile switch
                {
                    WorldTileCategory.Blank => TileCategory.Blank,
                    WorldTileCategory.Ocean => TileCategory.Water,
                    WorldTileCategory.Mountain => TileCategory.Wall,
                    WorldTileCategory.Grass => TileCategory.Floor,
                    WorldTileCategory.Forest => TileCategory.Floor,
                    WorldTileCategory.Desert => TileCategory.Floor,
                    _ => throw new InvalidEnumArgumentException()
                };
                _tiles[position] = new TileData(TileData.Build(tileType, false));
                result.Add((position, _tiles[position]));
            }
            _onTilesLoaded.OnNext(result);
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

        public bool IsPositionInsideMap(Vector2Int position)
        {
            return true;
        }

        public void UpdateTurn()
        {
            return;
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

        public void SetOverlayTiles(IEnumerable<Vector2Int> positions, OverlayTileCategory? category)
        {
            var result = new List<(Vector2Int position, OverlayTileCategory? category)>();
            foreach (var position in positions)
            {
                OverlayTileCategory? currentCategory =
                    _overlayTiles.ContainsKey(position) ? _overlayTiles[position] : null;
                if (category != currentCategory)
                {
                    if (category != null)
                    {
                        if (GetTile(position).MapOr(false,
                                tile => tile.TileType == category.Value.GetPlaceableTileCategory()))
                        {
                            _overlayTiles[position] = category.Value;
                            result.Add((position, category.Value));
                        }
                    }
                    else
                    {
                        _overlayTiles.Remove(position);
                        result.Add((position, null));
                    }
                }
            }

            _onOverlayTilesChanged.OnNext(result);
        }
    }
}