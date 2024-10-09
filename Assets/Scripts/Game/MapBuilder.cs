#nullable enable
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
using UnityEngine;
using Utilities;

namespace Game
{
    public class MapBuilder
    {
        private readonly Tilemap _tilemap;
        private readonly Location _location;
        private readonly List<CharacterMemento> _characters = new();
        private readonly List<ItemEntityMemento> _items = new();
        private readonly List<StairsMemento> _stairs = new();
        private readonly List<ChestMemento> _chests = new();
        private readonly List<TrapMemento> _traps = new();
        private EntityMemento? _bonfire;
        private readonly List<Id<IEntity>> _keyCharacters = new();
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly Vector2Int _upStairPosition;
        private readonly Vector2Int _downStairPosition;
        private readonly HashSet<Vector2Int> _blankPositions;

        public MapBuilder(TilemapMemento tilemapData, DungeonMapData data, Location location)
        {
            _tilemap = new Tilemap(tilemapData);
            _location = location;
            _blankPositions = _tilemap.GetAllWalkablePositions();

            var rooms = _tilemap.Rooms.ToList();

            if (Random.value < data.ShopChance && rooms.Count() > 1)
            {
                var shopRoom = rooms.GetAtRandom();
                rooms.Remove(shopRoom);
                _shop = CreateShop(data, shopRoom);
            }

            foreach (var room in rooms)
                AddGrasses(data, room);

            if (Random.value < data.MonsterHouseChance && rooms.Count() > 1)
            {
                var monsterHouseRoom = rooms.GetAtRandom();
                rooms.Remove(monsterHouseRoom);
                _monsterHouse = CreateMonsterHouse(data, monsterHouseRoom);
            }
            if (Random.value < data.RestRoomChance && rooms.Count() > 1)
            {
                var restRoom = rooms.GetAtRandom();
                rooms.Remove(restRoom);

                CreateRestRoom(data, restRoom);
            }

            _downStairPosition = GetRandomBlankPositionInRoom(rooms.GetAtRandom());
            _upStairPosition = GetRandomBlankPositionInRoom(rooms.GetAtRandom());

            if (data.existBoss)
            {
                var bossRoom = rooms.GetAtRandom();
                foreach (var bossData in data.Boss)
                {
                    var boss = CharacterFactory.BuildCharacter(bossData, GetRandomBlankPositionInRoom(bossRoom),
                        isSlept: false, isShiny: false);
                    _characters.Add(boss);
                    _keyCharacters.Add(new Id<IEntity>(boss.Entity.Id));
                }
            }

            foreach (var room in rooms)
            {
                CreateRoom(data, room);
            }
        }

        private void AddGrasses(DungeonMapData data, RectInt room)
        {
            var randomValue = Random.value * 1024;
            foreach (var position in room.RectRange())
            {
                if (data.GrassChance == 1 || Mathf.Clamp01(Mathf.PerlinNoise(position.x / 8f + randomValue, position.y / 8f + randomValue)) < data.GrassChance)
                {
                    _tilemap.SetGrasses(new[] { position }, true);
                }
            }
        }

        private int GetCount(float attemptCount)
        {
            var probability = 0.5f;
            return MathExtension.RandomBinomialApproxValue(attemptCount * 2, probability);
        }

        private void CreateRoom(DungeonMapData data, RectInt room)
        {
            var characterCount = GetCount(data.CharacterCount);
            var itemCount = GetCount(data.ItemCount);
            var chestCount = Random.value < data.ChestChance ? 1 : 0;
            var trapCount = GetCount(data.TrapCount);

            AddCharactersToRoom(data, room, characterCount);
            AddItemsToRoom(data, room, itemCount);
            AddChestsToRoom(data, room, chestCount);
            AddTrapsToRoom(data, room, trapCount);
        }

        private ShopMemento CreateShop(DungeonMapData data, RectInt room)
        {
            var shopItems = data.ShopItems.GetRandomItem().Items;

            var width = Random.Range(2, 5);
            var height = Random.Range(2, 5);
            var rect = room.GetCenteredInnerRect(new Vector2Int(width, height));

            foreach (var position in rect.RectRange())
            {
                _items.Add(ItemFactory.Build(position, Item.Build(shopItems.GetRandomItem(), ItemState.ShopItem)));
                _blankPositions.Remove(position);
            }

            var clerkPosition = GetRandomBlankPositionInRoom(room);
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false,
                homePosition: (_location, clerkPosition));
            _characters.Add(clerk);

            return Shop.Build(room, new(clerk.Entity.Id), _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(DungeonMapData data, RectInt room)
        {
            AddItemsToRoom(data, room, 5);
            AddChestsToRoom(data, room, 1);
            AddTrapsToRoom(data, room, 3);

            return MonsterHouse.Build(room);
        }

        private void CreateRestRoom(DungeonMapData data, RectInt room)
        {
            if (room.width >= 5 && room.height >= 5)
            {
                var innerRect = room.GetRandomInnerRect(new Vector2Int(5, 5));

                var center = innerRect.min + new Vector2Int(2, 2);

                _bonfire = Bonfire.Build(center);

                foreach (var position in innerRect.RectRange())
                {
                    _blankPositions.Remove(position);
                }

                foreach (var direction in DirectionMethods.AllDirections.GetAtRandom(Random.Range(1, 4)))
                {
                    var position = center + direction.Vector();
                    var character = CharacterFactory.BuildCharacter(data.Npcs.GetRandomItem(), position,
                        direction.Reverse(), Random.value < data.SleepChance, Random.value < data.ShinyChance,
                        homePosition: (_location, center));
                    _characters.Add(character);
                }
            }
            
            var itemCount = GetCount(data.ItemCount);
            var chestCount = Random.value < data.ChestChance ? 1 : 0;
            var trapCount = GetCount(data.TrapCount);

            AddItemsToRoom(data, room, itemCount);
            AddChestsToRoom(data, room, chestCount);
            AddTrapsToRoom(data, room, trapCount);
        }

        private Vector2Int GetRandomBlankPositionInRoom(RectInt room)
        {
            var position = _blankPositions.Where(position => room.Contains(position)).GetAtRandom();
            _blankPositions.Remove(position);
            return position;
        }

        private IEnumerable<Vector2Int> GetRandomBlankPositionsInRoom(RectInt room, int count)
        {
            var positions = _blankPositions.Where(position => room.Contains(position)).GetAtRandom(count);
            foreach (var position in positions)
                _blankPositions.Remove(position);
            return positions;
        }

        private void AddCharactersToRoom(DungeonMapData data, RectInt room, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(room, count))
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position,
                    isSlept: Random.value < data.SleepChance, isShiny: Random.value < data.ShinyChance);
                _characters.Add(character);
            }
        }

        private void AddItemsToRoom(DungeonMapData data, RectInt room, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(room, count))
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));
        }

        private void AddChestsToRoom(DungeonMapData data, RectInt room, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(room, count))
            {
                if (Random.value < data.MimicChance)
                {
                    _chests.Add(Chest.Build(position, data.Mimic));
                }
                else if (Random.value < data.WeaponChanceInChest)
                {
                    _chests.Add(Chest.Build(position, WeaponFactory.Create(data.Items.GetRandomItem(ItemCategory.Weapons), data.WeaponPrefixes.GetRandomItem())));
                }
                else
                {
                    _chests.Add(Chest.Build(position, data.ChestItems.GetRandomItem()));
                }
            }
        }

        private void AddTrapsToRoom(DungeonMapData data, RectInt room, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(room, count))
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
                _tilemap.Serialize(),
                _characters,
                _items,
                EventEntityManager.Build(_stairs, _chests, _traps, _bonfire.ToOption()),
                _keyCharacters.Select(key => key.ToString()).ToList(),
                _monsterHouse.ToOption(),
                _shop.ToOption(),
                _blankPositions.GetAtRandom()
            );
        }
    }
}