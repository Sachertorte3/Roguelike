using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using static RandomDungeonWithBluePrint.Constants;
using Random = UnityEngine.Random;

namespace Domain.Service.Map
{
    public class TilemapBuilder
    {
        private readonly int _width;
        private readonly TileCategory[] _tiles;
        private readonly HashSet<Vector2Int> _grasses;
        private readonly float _waterChance;
        private readonly float _randomValue;
        private readonly Dictionary<Id<Room>, RectInt> _rooms = new();
        public List<Id<Room>> RoomIds => _rooms.Keys.ToList();

        public TilemapBuilder(FieldBluePrint bluePrint, float waterChance)
        {
            var field = FieldBuilder.Build(bluePrint);
            _width = field.Grid.Size.x + 2;
            _tiles = new TileCategory[_width * (field.Grid.Size.y + 2)];
            _waterChance = waterChance;
            _randomValue = Random.value * 1024;
            var roomRects = field.Rooms.Select(room => room.Rect)
                .Select(rect => new RectInt(rect.position + new Vector2Int(1, 1), rect.size));

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
                            ? GetNotWalkableCategory(x, y)
                            : TileCategory.Floor;
                    }

                    _tiles[x + 1 + (y + 1) * _width] = tileType;
                }
            }

            _grasses = new();
            foreach (var room in roomRects)
            {
                var roomId = Id<Room>.Generate();
                _rooms[roomId] = room;
            }
        }

        private TileCategory GetNotWalkableCategory(int x, int y)
        {
            if (_waterChance == 1 || Mathf.Clamp01(Mathf.PerlinNoise(x / 16f + _randomValue, y / 16f + _randomValue)) < _waterChance)
            {
                return TileCategory.Water;
            }
            return TileCategory.Wall;
        }

        public void RoundRoomCorner(Id<Room> roomId)
        {
            var room = _rooms[roomId];
            RoundCorner(new Vector2Int(room.xMin, room.yMin), Direction8.DownLeft, room.size);
            RoundCorner(new Vector2Int(room.xMax - 1, room.yMin), Direction8.DownRight, room.size);
            RoundCorner(new Vector2Int(room.xMin, room.yMax - 1), Direction8.UpLeft, room.size);
            RoundCorner(new Vector2Int(room.xMax - 1, room.yMax - 1), Direction8.UpRight, room.size);
        }

        private void RoundCorner(Vector2Int position, Direction8 corner, Vector2Int roomsize)
        {
            var directionX = new Vector2Int(-corner.Vector().x, 0);
            var directionY = new Vector2Int(0, -corner.Vector().y);
            ProcessDirection(position, corner, roomsize.x / 2, directionX, directionY);
            ProcessDirection(position, corner, roomsize.y / 2, directionY, directionX);
        }

        private void ProcessDirection(Vector2Int position, Direction8 corner, int maxDimension, Vector2Int primaryDirection, Vector2Int secondaryDirection)
        {
            int secondaryIncrement = 0;
            while (maxDimension >= 1)
            {
                var randDimension = Random.Range(1, maxDimension);
                for (int primaryIncrement = 0; primaryIncrement < randDimension; primaryIncrement++)
                {
                    var newPosition = position + primaryIncrement * primaryDirection + secondaryIncrement * secondaryDirection;
                    if (IsSafeToPlaceWall(newPosition, corner))
                        _tiles[newPosition.x + newPosition.y * _width] = GetNotWalkableCategory(newPosition.x, newPosition.y);
                    else
                        break;
                }
                maxDimension /= 2;
                secondaryIncrement += 1;
            }
        }

        private bool IsSafeToPlaceWall(Vector2Int position, Direction8 corner)
        {
            if (position.x + corner.Vector().x < 0
            || position.y + corner.Vector().y < 0
            || position.x + corner.Vector().x >= _width
            || position.y + corner.Vector().y >= _tiles.Length / _width)
            {
                return false;
            }
            if (_tiles[position.x + corner.Vector().x + position.y * _width] == TileCategory.Floor
            || _tiles[position.x + (position.y + corner.Vector().y) * _width] == TileCategory.Floor)
            {
                return false;
            }
            return true;
        }

        public HashSet<Vector2Int> GetWalkablePositionsIn(Id<Room> roomId)
        {
            var room = _rooms[roomId];
            return room.RectRange().Where(position => _tiles[position.x + position.y * _width] == TileCategory.Floor).ToHashSet();
        }

        public RectInt GetRoom(Id<Room> roomId) => _rooms[roomId];

        public RectInt? GetCenteredInnerRect(Id<Room> roomId, Vector2Int size)
        {
            var rect = _rooms[roomId].GetCenteredInnerRect(size);
            if (rect.RectRange().Any(position => _tiles[position.x + position.y * _width] != TileCategory.Floor))
            {
                return null;
            }
            return rect;
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            foreach (var position in positions)
            {
                if (isGrass != _grasses.Contains(position))
                {
                    if (isGrass)
                    {
                        if (_tiles[position.x + position.y * _width] == TileCategory.Floor)
                        {
                            _grasses.Add(position);
                        }
                    }
                    else
                    {
                        _grasses.Remove(position);
                    }
                }
            }
        }

        public HashSet<Vector2Int> GetAllWalkablePositions()
        {
            return _tiles.Select((tile, index) => (tile, index))
                .Where(tile => tile.tile == TileCategory.Floor)
                .Select(tile => new Vector2Int(tile.index % _width, tile.index / _width))
                .ToHashSet();
        }

        public TilemapMemento Build()
        {
            return new TilemapMemento(
                _width,
                _tiles.Select(tile => TileData.Build(tile, false)).ToArray(),
                _grasses
            );
        }
    }
}