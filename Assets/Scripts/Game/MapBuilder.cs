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
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Game
{
    public class MapBuilder
    {
        private readonly Tilemap _tilemap;
        private readonly List<CharacterMemento> _characters;
        private readonly List<ItemEntityMemento> _items;
        private readonly List<StairsMemento> _stairs;
        private readonly List<ChestMemento> _chests;
        private readonly List<TrapMemento> _traps;
        private readonly Option<EntityMemento> _bonfire;
        private readonly List<Id<IEntity>> _keyCharacters;
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly Vector2Int _upStairPosition;
        private readonly Vector2Int _downStairPosition;
        private Vector2Int? _bonfirePosition;
        private readonly List<Vector2Int> _randomBlankPositions;

        public MapBuilder(TilemapMemento tilemapData, DungeonMapData data)
        {
            _tilemap = new Tilemap(tilemapData);
            _characters = new List<CharacterMemento>();
            _items = new List<ItemEntityMemento>();
            _keyCharacters = new List<Id<IEntity>>();
            _stairs = new List<StairsMemento>();
            _chests = new List<ChestMemento>();
            _traps = new List<TrapMemento>();
            _randomBlankPositions = new List<Vector2Int>();

            var rooms = _tilemap.Rooms.ToList();

            foreach (var room in rooms)
                AddGrasses(data, room);

            _shop = CreateShop(data, rooms);
            _monsterHouse = CreateMonsterHouse(data, rooms);
            CreateRestRoom(data, rooms);

            var downStairsRoom = rooms.GetAtRandom();
            var upStairsRoom = rooms.GetAtRandom();
            RectInt? bossRoom = data.existBoss ? rooms.GetAtRandom() : null;

            foreach (var room in rooms)
            {
                var characterCount = GetCount(data.CharacterCount);
                var itemCount = GetCount(data.ItemCount);
                var weaponCount = GetCount(data.WeaponCount);
                var chestCount = Random.value < data.ChestChance ? 1 : 0;
                var trapCount = GetCount(data.TrapCount);
                var bossCount = data.existBoss ? data.Boss.Count : 0;
                var sum = characterCount + itemCount + weaponCount + chestCount + trapCount + bossCount + 3;

                var positions = room.RectRange().ToList();
                if (positions.Count < sum)
                {
                    Log.Error("positions.Count < sum");
                }

                var characterPositions = positions.GetAtRandomAndRemove(characterCount);
                var itemPositions = positions.GetAtRandomAndRemove(itemCount);
                var weaponPositions = positions.GetAtRandomAndRemove(weaponCount);
                var chestPositions = positions.GetAtRandomAndRemove(chestCount);
                var trapPositions = positions.GetAtRandomAndRemove(trapCount);

                AddCharactersToRoom(data, characterPositions);
                AddItemsToRoom(data, itemPositions);
                AddWeaponsToRoom(data, weaponPositions);
                AddChestsToRoom(data, chestPositions, _chests);
                AddTrapsToRoom(data, trapPositions, _traps);

                if (room == bossRoom)
                {
                    foreach (var bossData in data.Boss)
                    {
                        var boss = CharacterFactory.BuildCharacter(bossData, positions.GetAtRandomAndRemove(1).First(),
                            isSlept: false, isShiny: false);
                        _characters.Add(boss);
                        _keyCharacters.Add(new Id<IEntity>(boss.Entity.Id));
                    }
                }

                if (room == downStairsRoom)
                {
                    _downStairPosition = positions.GetAtRandomAndRemove(1).First();
                }

                if (room == upStairsRoom)
                {
                    _upStairPosition = positions.GetAtRandomAndRemove(1).First();
                }

                _randomBlankPositions.Add(positions.GetAtRandomAndRemove(1).First());
            }

            _bonfire =
                (_bonfirePosition != null ? Option.Some(_bonfirePosition!.Value) : Option.None<Vector2Int>()).Map(
                    position => Bonfire.Build(position));
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
            return MathExtension.RandomBinomialApproxValue(attemptCount, probability);
        }

        private ShopMemento? CreateShop(DungeonMapData data, List<RectInt> rooms)
        {
            if (Random.value >= data.ShopChance || rooms.Count() <= 1) return null;

            var shopRoom = rooms.GetAtRandom();
            rooms.Remove(shopRoom);

            _tilemap.SetGrasses(shopRoom.RectRange(), false);

            var shopItems = data.ShopItems.GetRandomItem().Items;

            var positions = shopRoom.RectRange().GetAtRandom(6).ToList();

            var width = Random.Range(2, 5);
            var height = Random.Range(2, 5);
            var rect = shopRoom.GetCenteredInnerRect(new Vector2Int(width, height));

            foreach (var position in rect.RectRange())
            {
                _items.Add(ItemFactory.Build(position, Item.Build(shopItems.GetRandomItem(), ItemState.ShopItem)));
                positions.Remove(position);
            }

            var clerkPosition = positions.Last();
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false,
                homePosition: clerkPosition);
            _characters.Add(clerk);
            return Shop.Build(shopRoom, clerk.Entity, _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(DungeonMapData data, List<RectInt> rooms)
        {
            if (Random.value >= data.MonsterHouseChance || rooms.Count() <= 1) return null;

            var monsterHouseRoom = rooms.GetAtRandom();
            rooms.Remove(monsterHouseRoom);

            var positions = monsterHouseRoom.RectRange().ToList();
            foreach (var position in positions.GetAtRandomAndRemove(5))
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));
            AddChestsToRoom(data, positions.GetAtRandomAndRemove(1), _chests);

            return MonsterHouse.Build(monsterHouseRoom);
        }

        private void CreateRestRoom(DungeonMapData data, List<RectInt> rooms)
        {
            if (Random.value >= data.RestRoomChance || rooms.Count() <= 1) return;

            var restRoom = rooms.GetAtRandom();
            rooms.Remove(restRoom);

            var positions = restRoom.RectRange().ToList();

            if (restRoom.width >= 5 && restRoom.height >= 5)
            {
                var innerRect = restRoom.GetRandomInnerRect(new Vector2Int(5, 5));

                var center = Vector2Int.RoundToInt(innerRect.center);

                _bonfirePosition = center;

                foreach (var position in innerRect.RectRange())
                {
                    positions.Remove(position);
                }

                foreach (var direction in DirectionMethods.AllDirections.GetAtRandom(Random.Range(1, 4)))
                {
                    var position = center + direction.Vector();
                    var character = CharacterFactory.BuildCharacter(data.Npcs.GetRandomItem(), position,
                        direction.Reverse(), Random.value < data.SleepChance, Random.value < data.ShinyChance,
                        homePosition: center);
                    _characters.Add(character);
                }
            }

            foreach (var position in positions.GetAtRandomAndRemove(2))
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));
            AddChestsToRoom(data, positions.GetAtRandomAndRemove(1), _chests);
        }

        private void AddCharactersToRoom(DungeonMapData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position,
                    isSlept: Random.value < data.SleepChance, isShiny: Random.value < data.ShinyChance);
                _characters.Add(character);
            }
        }

        private void AddItemsToRoom(DungeonMapData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));
        }

        private void AddWeaponsToRoom(DungeonMapData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var material = data.Materials.GetRandomItem();
                var mold = data.WeaponMolds.GetRandomItem();
                if (Random.value < data.PrefixChance)
                {
                    var prefix = data.WeaponPrefixes.GetRandomItem();
                    var weapon = WeaponFactory.Create(prefix, material, mold);
                    _items.Add(ItemFactory.Build(position, Item.Build(weapon)));
                }
                else
                {
                    var weapon = WeaponFactory.Create(material, mold);
                    _items.Add(ItemFactory.Build(position, Item.Build(weapon)));
                }
            }
        }

        private void AddChestsToRoom(DungeonMapData data, List<Vector2Int> positions, List<ChestMemento> chests)
        {
            foreach (var position in positions)
            {
                if (Random.value < data.MimicChance)
                {
                    chests.Add(Chest.Build(position, data.Mimic));
                }
                else if (Random.value < data.WeaponChanceInChest)
                {
                    var material = data.Materials.GetRandomItem();
                    var mold = data.WeaponMolds.GetRandomItem();
                    var prefix = data.WeaponPrefixes.GetRandomItem();
                    var weapon = WeaponFactory.Create(prefix, material, mold);
                    chests.Add(Chest.Build(position, weapon));
                }
                else
                {
                    var item = data.ChestItems.GetRandomItem();
                    chests.Add(Chest.Build(position, item));
                }
            }
        }

        private void AddTrapsToRoom(DungeonMapData data, List<Vector2Int> positions, List<TrapMemento> traps)
        {
            foreach (var position in positions)
            {
                traps.Add(Trap.Build(data.Traps.GetRandomItem(), position));
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

        public MapMemento Build()
        {
            return new MapMemento
            (
                _tilemap.Serialize(),
                _characters,
                _items,
                EventEntityManager.Build(_stairs, _chests, _traps, _bonfire),
                _keyCharacters.Select(key => key.ToString()).ToList(),
                _monsterHouse.ToOption(),
                _shop.ToOption(),
                _randomBlankPositions.GetAtRandom()
            );
        }
    }
}