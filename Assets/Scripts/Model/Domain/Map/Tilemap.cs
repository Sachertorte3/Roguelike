using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;
using static RandomDungeonWithBluePrint.Constants;

namespace Model.Domain.Map
{
    public class Tilemap : ITilemapViewer
    {
        private readonly HashSet<Vector2Int> _allPassablePositionsSet;
        private readonly Subject<(Vector2Int, TileData)> _onChangeTile = new();
        private readonly ObservableDictionary<Vector2Int, TileData> _tiles;
        public readonly int Height;
        public readonly int Width;

        [Inject]
        public Tilemap(FieldBluePrint bluePrint)
        {
            var field = FieldBuilder.Build(bluePrint);
            Width = field.Grid.Size.x;
            Height = field.Grid.Size.y;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(
                new RectInt(0, 0, Width, Height)
                    .RectRange().ToDictionary(
                        position => position,
                        position =>
                        {
                            var mapChipType = field.Grid[position.x, position.y];
                            var tileType = mapChipType == (int)MapChipType.Wall
                                ? TileCategory.Wall
                                : TileCategory.Floor;
                            return new TileData(tileType);
                        }
                    )
            );
            _tiles.ObserveReplace()
                .Subscribe(context => _onChangeTile.OnNext((context.NewValue.Key, context.NewValue.Value)));
            _allPassablePositionsSet = FindAllPassablePositions().ToHashSet();
            OnChangeTile.Subscribe(changeTile =>
            {
                if (changeTile.tile.IsPassable())
                    _allPassablePositionsSet.Add(changeTile.position);
                else
                    _allPassablePositionsSet.Remove(changeTile.position);
            });
        }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ObservableDictionary<Vector2Int, TileData>(Rect.RectRange()
                .ToDictionary(x => x, _ => new TileData(TileCategory.Blank)));
            _tiles.ObserveReplace()
                .Subscribe(context => _onChangeTile.OnNext((context.NewValue.Key, context.NewValue.Value)));
        }

        public Vector2Int Size => new(Width, Height);
        public Observable<(Vector2Int position, TileData tile)> OnChangeTile => _onChangeTile;
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
    }
}