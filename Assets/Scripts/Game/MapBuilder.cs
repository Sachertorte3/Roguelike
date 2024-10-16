#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Map;
using Domain.Service.Rooms;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Game
{
    public class MapBuilder
    {
        private readonly TilemapBuilder _tilemap;
        private readonly Location _location;
        private readonly List<CharacterMemento> _characters = new();
        private readonly List<ItemEntityMemento> _items = new();
        private readonly List<StairsMemento> _stairs = new();
        private readonly List<ChestMemento> _chests = new();
        private readonly List<TrapMemento> _traps = new();
        private readonly List<MoneyMemento> _money = new();
        private EntityMemento? _bonfire;
        private readonly List<Id<IEntity>> _keyCharacters = new();
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly Vector2Int _upStairPosition;
        private readonly Vector2Int _downStairPosition;
        private readonly Dictionary<Id<Room>, HashSet<Vector2Int>> _blankPositionsInRooms;

        public MapBuilder(FieldBluePrint bluePrint, float waterChance, DungeonMapData data, Location location)
        {
            _tilemap = new TilemapBuilder(bluePrint, waterChance);
            _location = location;
            _blankPositionsInRooms = new();

            var roomIds = _tilemap.RoomIds.ToList();

            if (Random.value < data.ShopChance && roomIds.Count() > 1)
            {
                var shopRoom = roomIds.GetAtRandom();
                _shop = CreateShop(data, shopRoom);
                if (_shop != null)
                    roomIds.Remove(shopRoom);
            }

            if (Random.value < data.MonsterHouseChance && roomIds.Count() > 1)
            {
                var monsterHouseRoom = roomIds.GetAtRandom();
                _monsterHouse = CreateMonsterHouse(data, monsterHouseRoom);
                if (_monsterHouse != null)
                    roomIds.Remove(monsterHouseRoom);
            }
            if (Random.value < data.RestRoomChance && roomIds.Count() > 1)
            {
                var restRoom = roomIds.GetAtRandom();
                if (CreateRestRoom(data, restRoom))
                    roomIds.Remove(restRoom);
            }

            foreach (var room in roomIds)
            {
                CreateRoom(data, room);
            }

            _downStairPosition = GetRandomBlankPositionInRoom(roomIds.GetAtRandom());
            _upStairPosition = GetRandomBlankPositionInRoom(roomIds.GetAtRandom());

            if (data.ExistBoss)
            {
                var bossRoom = roomIds.GetAtRandom();
                foreach (var bossData in data.Boss)
                {
                    var boss = CharacterFactory.BuildCharacter(bossData, GetRandomBlankPositionInRoom(bossRoom),
                        isSlept: false, isShiny: false);
                    _characters.Add(boss);
                    _keyCharacters.Add(new Id<IEntity>(boss.Entity.Id));
                }
            }

            foreach (var room in roomIds)
                AddGrasses(data, room);
        }

        private HashSet<Vector2Int> GetAllBlankPositionInRoom(Id<Room> roomId)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            return _blankPositionsInRooms[roomId];
        }
        private Vector2Int GetRandomBlankPositionInRoom(Id<Room> roomId)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            var position = _blankPositionsInRooms[roomId].GetAtRandom();
            _blankPositionsInRooms[roomId].Remove(position);
            return position;
        }

        private IEnumerable<Vector2Int> GetRandomBlankPositionsInRoom(Id<Room> roomId, int count)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            var positions = _blankPositionsInRooms[roomId].GetAtRandom(count);
            foreach (var position in positions)
                _blankPositionsInRooms[roomId].Remove(position);
            return positions;
        }

        private void AddGrasses(DungeonMapData data, Id<Room> roomId)
        {
            var randomValue = Random.value * 1024;
            foreach (var position in _tilemap.GetWalkablePositionsIn(roomId))
            {
                if (data.GrassChance == 1 || Mathf.Clamp01(Mathf.PerlinNoise(position.x / 8f + randomValue, position.y / 8f + randomValue)) < data.GrassChance)
                {
                    _tilemap.SetGrasses(new[] { position }, true);
                }
            }
        }

        private void AddIce(Vector2Int position)
        {
            _tilemap.SetIces(new[] { position }, true);
        }

        private void CreateRoom(DungeonMapData data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var chestCount = Random.value < data.ChestChance ? 1 : 0;
            var characterCount = data.CharacterCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddChestsToRoom(data, roomId, chestCount);
            AddCharactersToRoom(data, roomId, characterCount);
            AddTrapsToRoom(data, roomId, trapCount);
        }

        private ShopMemento? CreateShop(DungeonMapData data, Id<Room> roomId)
        {
            var shopItems = data.ShopItems.GetRandomItem().Items;

            var width = Random.Range(2, 5);
            var height = Random.Range(2, 5);
            var rect = _tilemap.GetCenteredInnerRect(roomId, new Vector2Int(width, height));

            if (rect == null)
            {
                return null;
            }

            foreach (var position in rect.Value.RectRange())
            {
                var item = shopItems.GetRandomItem();
                _items.Add(ItemFactory.Build(position, Item.Build(item, ItemState.ShopItem)));
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            var clerkPosition = GetRandomBlankPositionInRoom(roomId);
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false,
                homePosition: (_location, clerkPosition));
            _characters.Add(clerk);

            return Shop.Build(_tilemap.GetRoom(roomId), new(clerk.Entity.Id), _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(DungeonMapData data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            AddItemsToRoom(data, roomId, 5);
            AddMoneyToRoom(data, roomId, 3);
            AddChestsToRoom(data, roomId, 1);
            AddTrapsToRoom(data, roomId, 3);

            return MonsterHouse.Build(_tilemap.GetRoom(roomId));
        }

        private bool CreateRestRoom(DungeonMapData data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            var innerRect = _tilemap.GetWalkablePositionsIn(roomId).GetRandomInnerRect(new Vector2Int(5, 5));
            if (innerRect == null)
            {
                return false;
            }

            var center = innerRect.Value.min + new Vector2Int(2, 2);

            _bonfire = Bonfire.Build(center);

            foreach (var position in innerRect.Value.RectRange())
            {
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            foreach (var direction in DirectionMethods.AllDirections.GetAtRandom(Random.Range(1, 4)))
            {
                var position = center + direction.Vector();
                var character = CharacterFactory.BuildCharacter(data.Npcs.GetRandomItem(), position,
                    direction.Reverse(), Random.value < data.SleepChance, Random.value < data.ShinyChance,
                    homePosition: (_location, center));
                _characters.Add(character);
            }

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var chestCount = Random.value < data.ChestChance ? 1 : 0;
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddChestsToRoom(data, roomId, chestCount);
            AddTrapsToRoom(data, roomId, trapCount);

            return true;
        }

        private void AddCharactersToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position,
                    isSlept: Random.value < data.SleepChance, isShiny: Random.value < data.ShinyChance);
                _characters.Add(character);
            }
        }

        private void AddItemsToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                var item = data.ItemDatabase.GetRandomItem();
                _items.Add(ItemFactory.Build(position, Item.Build(item)));
            }
        }

        public void AddMoneyToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                _money.Add(Money.Build(position, data.MoneyAmount()));
            }
        }

        private void AddChestsToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                if (Random.value < data.MimicChance)
                {
                    _chests.Add(Chest.Build(position, data.Mimic));
                }
                else if (Random.value < data.WeaponChanceInChest)
                {
                    var weapon = data.ItemDatabase.GetRandomItem(ItemCategory.Weapons);
                    _chests.Add(Chest.Build(position, WeaponFactory.Create(weapon, data.WeaponPrefixes.GetRandomItem())));
                }
                else
                {
                    _chests.Add(Chest.Build(position, data.ChestItems.GetRandomItem()));
                }
            }
        }

        private void AddTrapsToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                _traps.Add(Trap.Build(data.Traps.GetRandomItem(), position));
            }
        }

        public void AddUpStairs(DungeonMapData data, int level, Id<IEntity>? upStairsId,
            Id<IEntity>? upStairsDestinationId)
        {
            if (upStairsId != null && upStairsDestinationId != null)
                _stairs.Add(Stairs.Build(MovementEntityType.UpStairs, _upStairPosition, upStairsId,
                    new Location(data.Name, level - 1), upStairsDestinationId));
            else
                _stairs.Add(Stairs.Build(MovementEntityType.UpStairs, _upStairPosition,
                    new Location(data.Name, level - 1)));
        }

        public void AddDownStairs(DungeonMapData data, int level, Id<IEntity>? downStairsId,
            Id<IEntity>? downStairsDestinationId)
        {
            if (downStairsId != null && downStairsDestinationId != null)
                _stairs.Add(Stairs.Build(MovementEntityType.DownStairs, _downStairPosition, downStairsId,
                    new Location(data.Name, level + 1), downStairsDestinationId));
            else
                _stairs.Add(Stairs.Build(MovementEntityType.DownStairs, _downStairPosition,
                    new Location(data.Name, level + 1)));
        }

        public MapMemento Build(Id<IMap> id)
        {
            return new MapMemento
            (
                id,
                _location,
                _tilemap.Build(),
                _characters,
                _items,
                EventEntityManager.Build(_stairs, _chests, _traps, _money, _bonfire.ToOption()),
                FireEntityManager.Build(),
                _keyCharacters.Select(key => key.ToString()).ToList(),
                _monsterHouse.ToOption(),
                _shop.ToOption(),
                _blankPositionsInRooms.Values.SelectMany(positions => positions).GetAtRandom()
            );
        }
    }
}