#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
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
using Utilities.Serialize.Option;

namespace Game
{
    public class MapBuilder
    {
        private readonly Id<IMap> _mapId;
        private readonly TilemapBuilder _tilemap;
        private readonly List<CharacterMemento> _characters = new();
        private readonly List<ItemEntityMemento> _items = new();
        private readonly List<StairsMemento> _stairs = new();
        private readonly List<ChestMemento> _chests = new();
        private readonly List<TrapMemento> _traps = new();
        private readonly List<StatueMemento> _statues = new();
        private readonly List<MoneyMemento> _money = new();
        private BonfireMemento? _bonfire;
        private MagicPotMemento? _magicPot;
        private EntityMemento? _teleporter;
        private readonly List<Id<IEntity>> _keyCharacters = new();
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly List<Id<Room>> _canPlaceStairRooms = new();
        private readonly Dictionary<Id<Room>, HashSet<Vector2Int>> _blankPositionsInRooms;

        public MapBuilder(FieldBluePrint bluePrint, float waterChance, DungeonMapData data, Id<IMap> mapId)
        {
            _tilemap = new TilemapBuilder(data.Type, bluePrint, waterChance);
            _mapId = mapId;
            _blankPositionsInRooms = new Dictionary<Id<Room>, HashSet<Vector2Int>>();

            var roomIds = _tilemap.RoomIds.ToList();
            var grassRoomIds = _tilemap.RoomIds.ToList();

            // 孤立したSectionを検出してIsolateRoomとして処理（最初に処理）
            foreach (var isolateRoomId in _tilemap.IsolateRooms)
            {
                if (roomIds.Count() > 1)
                {
                    CreateIsolateRoom(data, isolateRoomId);
                    roomIds.Remove(isolateRoomId); // 通常の部屋処理から除外
                }
            }

            if (Random.value < data.ShopChance && roomIds.Count() > 1)
            {
                var shopRoom = roomIds.GetAtRandom();
                _shop = CreateShop(data, shopRoom);
                if (_shop != null)
                {
                    roomIds.Remove(shopRoom);
                    grassRoomIds.Remove(shopRoom);
                }
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

            if (Random.value < data.LakeChance && roomIds.Count() > 1)
            {
                var lakeRoom = roomIds.GetAtRandom();
                if (CreateLakeRoom(data, lakeRoom))
                    roomIds.Remove(lakeRoom);
            }

            foreach (var room in roomIds)
            {
                CreateRoom(data, room);
            }

            _canPlaceStairRooms = roomIds.ToList();

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

            foreach (var room in grassRoomIds)
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

        public Vector2Int GetRandomStairPosition()
        {
            return GetRandomBlankPositionInRoom(_canPlaceStairRooms.GetAtRandom());
        }

        private void AddGrasses(DungeonMapData data, Id<Room> roomId)
        {
            var randomValue = Random.value * 1024;
            foreach (var position in _tilemap.GetWalkablePositionsIn(roomId))
            {
                if (data.GrassChance == 1 ||
                    Mathf.Clamp01(Mathf.PerlinNoise(position.x / 8f + randomValue, position.y / 8f + randomValue)) <
                    data.GrassChance)
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
            else if (data.CaveInOneRoom)
                _tilemap.CaveInOneRoom(roomId);

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var characterCount = data.CharacterCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddCharactersToRoom(data, roomId, characterCount);
            AddTrapsToRoom(data, roomId, trapCount);
            if (Random.value < data.StatueChance)
                AddStatueToRoom(data, roomId);
        }

        private bool CreateLakeRoom(DungeonMapData data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            var lakeSize = new Vector2Int(Random.Range(4, 6), Random.Range(4, 6));
            var innerRect = _tilemap.GetWalkablePositionsIn(roomId).GetRandomInnerRect(lakeSize + Vector2Int.one * 2);
            if (innerRect == null)
            {
                return false;
            }

            var lakeRect = new RectInt(innerRect.Value.min + Vector2Int.one, lakeSize);
            var islandRect = new RectInt(lakeRect.min + Vector2Int.one, lakeSize - Vector2Int.one * 2);
            _tilemap.SetWater(lakeRect.RectRange().Where(position => !islandRect.Contains(position)));
            GetAllBlankPositionInRoom(roomId).RemoveWhere(position => lakeRect.Contains(position));

            var teleporterPosition = islandRect.RectRange().GetAtRandom();
            _teleporter = Teleporter.Build(teleporterPosition);
            var itemPositions = islandRect.RectRange().Where(position => position != teleporterPosition);
            foreach (var position in itemPositions)
            {
                var item = data.ItemDatabase.GetRandomItem(data.Progress);
                _items.Add(ItemFactory.Build(position, item.Build()));
            }

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var characterCount = data.CharacterCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddCharactersToRoom(data, roomId, characterCount);
            AddTrapsToRoom(data, roomId, trapCount);
            if (Random.value < data.StatueChance)
                AddStatueToRoom(data, roomId);

            return true;
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
                var item = shopItems.GetRandomItem(data.Progress);
                _items.Add(ItemFactory.Build(position, Item.Build(item, state: ItemState.ShopItem)));
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            var clerkPosition = GetRandomBlankPositionInRoom(roomId);
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false,
                homeLocation: new Location(_mapId, clerkPosition));
            _characters.Add(clerk);

            return Shop.Build(_tilemap.GetRoom(roomId), new Id<IEntity>(clerk.Entity.Id), _items.ToList());
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

            var center = innerRect.Value.min + VectorExtension.FloorToInt(innerRect.Value.size / 2);

            if (Random.value < 0.5)
                _bonfire = Bonfire.Build(center);
            else
                _magicPot = MagicPot.Build(center);

            foreach (var position in innerRect.Value.RectRange())
            {
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            foreach (var direction in DirectionMethods.AllDirections.GetAtRandom(Random.Range(1, 3)))
            {
                var position = center + direction.Vector();
                var character = CharacterFactory.BuildCharacter(data.Npcs.GetRandomItem(), position,
                    direction.Reverse(), Random.value < data.SleepChance, Random.value < data.ShinyChance,
                    homeLocation: new Location(_mapId, center));
                _characters.Add(character);
            }

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddTrapsToRoom(data, roomId, trapCount);

            return true;
        }

        private void AddCharactersToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                if (data.Enemies.Count == 0)
                    break;
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position,
                    isSlept: Random.value < data.SleepChance, isShiny: Random.value < data.ShinyChance);
                _characters.Add(character);
            }
        }

        private void AddItemsToRoom(DungeonMapData data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                var item = data.ItemDatabase.GetRandomItem(data.Progress);
                _items.Add(ItemFactory.Build(position, item.Build()));
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
                    var weapon = data.ItemDatabase.GetRandomItem(ItemCategory.Weapons, data.Progress);
                    if (weapon is DirectWeaponData directWeapon)
                    {
                        _chests.Add(
                            Chest.Build(position, DirectWeapon.Build(directWeapon, data.WeaponPrefixes.GetRandomItem(data.Progress), isCursed: false)));
                    }
                    else
                    {
                        _chests.Add(Chest.Build(position, weapon));
                    }
                }
                else
                {
                    _chests.Add(Chest.Build(position, data.ChestItems.GetRandomItem(data.Progress)));
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

        private void AddStatueToRoom(DungeonMapData data, Id<Room> roomId)
        {
            var position = GetRandomBlankPositionInRoom(roomId);
            _statues.Add(Statue.Build(data.Statues.GetRandomItem(), position));
        }

        private bool CreateIsolateRoom(DungeonMapData data, Id<Room> roomId)
        {
            // テレポーターを先に配置（空白位置管理を使用）
            var teleporterPosition = GetRandomBlankPositionInRoom(roomId);
            _teleporter = Teleporter.Build(teleporterPosition);

            // 残りの位置をMoneyで埋める（空白位置管理を使用）
            var walkablePositions = _tilemap.GetWalkablePositionsIn(roomId);
            var moneyPositions = walkablePositions.Where(p => p != teleporterPosition).ToList();
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, moneyPositions.Count))
            {
                _money.Add(Money.Build(position, data.MoneyAmount()));
            }

            return true;
        }

        public void AddMovementEntity(MovementData data)
        {
            if (data.Id != null && data.DestinationId != null)
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(), data.Id,
                    data.Destination, data.DestinationId));
            else
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(),
                    data.Destination));
        }

        public MapMemento Build()
        {
            return new MapMemento
            (
                _mapId,
                _tilemap.Build(),
                new EntitiesMemento(
                    _characters,
                    _items,
                    EventEntityManager.Build(
                        _stairs,
                        _chests,
                        _traps,
                        _statues,
                        _money,
                        _bonfire.ToOption(),
                        _magicPot.ToOption(),
                        _teleporter.ToOption()),
                    FireEntityManager.Build()),
                _keyCharacters.Select(key => key.ToString()).ToList(),
                _monsterHouse.ToOption(),
                _shop.ToOption(),
                _blankPositionsInRooms.Values.SelectMany(positions => positions).GetAtRandom()
            );
        }
    }
}