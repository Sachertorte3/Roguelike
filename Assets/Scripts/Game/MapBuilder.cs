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
using UnityEngine;
using Unity.Logging;
using Utilities;
using Random = UnityEngine.Random;
using Domain.Service.Rooms;

namespace Game
{
    public class MapBuilder
    {
        private readonly Tilemap _tilemap;
        private readonly List<CharacterMemento> _characters;
        private readonly List<ItemEntityMemento> _items;
        private readonly List<StairsMemento> _stairs;
        private readonly List<ChestMemento> _chests;
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
            _tilemap = new(tilemapData);
            _characters = new();
            _items = new();
            _keyCharacters = new();
            _stairs = new();
            _chests = new();
            _randomBlankPositions = new();

            var rooms = _tilemap.Rooms.ToList();

            _shop = CreateShop(data, rooms);
            _monsterHouse = CreateMonsterHouse(data, rooms);
            CreateRestRoom(data, rooms);

            RectInt downStairsRoom = rooms.GetAtRandom();
            RectInt upStairsRoom = rooms.GetAtRandom();
            RectInt? bossRoom = data.existBoss ? rooms.GetAtRandom() : null;

            foreach (var room in rooms)
            {
                var characterCount = GetCount(data.CharacterCount);
                var itemCount = GetCount(data.ItemCount);
                var weaponCount = GetCount(data.WeaponCount);
                var chestCount = Random.value < data.ChestChance ? 1 : 0;
                var bossCount = data.existBoss ? data.Boss.Count : 0;
                var sum = characterCount + itemCount + weaponCount + chestCount + bossCount + 3;

                var positions = room.RectRange().ToList();
                if (positions.Count < sum)
                {
                    Log.Error("positions.Count < sum");
                }
                var characterPositions = positions.GetAtRandomAndRemove(characterCount);
                var itemPositions = positions.GetAtRandomAndRemove(itemCount);
                var weaponPositions = positions.GetAtRandomAndRemove(weaponCount);
                var chestPositions = positions.GetAtRandomAndRemove(chestCount);

                AddCharactersToRoom(data, characterPositions);
                AddItemsToRoom(data, itemPositions);
                AddWeaponsToRoom(data, weaponPositions);
                AddChestsToRoom(data, chestPositions, _chests);

                if (room == bossRoom)
                {
                    foreach (var bossData in data.Boss)
                    {
                        var boss = CharacterFactory.BuildCharacter(bossData, positions.GetAtRandomAndRemove(1).First(), isSlept: false, isShiny: false);
                        _characters.Add(boss);
                        _keyCharacters.Add(new(boss.Entity.Id));
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

            _bonfire = (_bonfirePosition != null ? Option.Some(_bonfirePosition!.Value): Option.None<Vector2Int>()).Map(position => Bonfire.Build(position));
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

            var shopItems = data.ShopItems.GetRandomItem().Items;

            var positions = shopRoom.RectRange().GetAtRandom(6).ToList();
            foreach (var position in positions.Take(5))
                _items.Add(ItemFactory.Build(position, Item.Build(shopItems.GetRandomItem(), ItemState.ShopItem)));

            var clerkPosition = positions.Last();
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false, homePosition: clerkPosition);
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
            _chests.Add(Chest.Build(positions.GetAtRandomAndRemove(1).First(), data.ChestItems.GetRandomItem()));

            return MonsterHouse.Build(monsterHouseRoom);
        }

        private void CreateRestRoom(DungeonMapData data, List<RectInt> rooms)
        {
            if (Random.value >= data.RestRoomChance || rooms.Count() <= 1) return;

            var restRoom = rooms.GetAtRandom();
            rooms.Remove(restRoom);

            var positions = restRoom.RectRange().ToList();

            var center = new Vector2Int(
                x: Random.value < 0.5f ? Mathf.CeilToInt(restRoom.center.x) : Mathf.FloorToInt(restRoom.center.x),
                y: Random.value < 0.5f ? Mathf.CeilToInt(restRoom.center.y) : Mathf.FloorToInt(restRoom.center.y)
            );

            if (restRoom.Contains(center - Vector2Int.one) && restRoom.Contains(center + Vector2Int.one))
            {
                _bonfirePosition = center;
                var rect = new RectInt(center - Vector2Int.one, Vector2Int.one * 3);
                foreach (var position in rect.RectRange())
                {
                    positions.Remove(position);
                }

                foreach (var position in rect.RectRange().Where(pos => pos != center).GetAtRandom(3))
                {
                    var direction = DirectionMethods.FromVector(center - position);
                    var character = CharacterFactory.BuildCharacter(data.Npcs.GetRandomItem(), position, direction, Random.value < data.SleepChance, Random.value < data.ShinyChance, homePosition: center);
                    _characters.Add(character);
                }
            }

            foreach (var position in positions.GetAtRandomAndRemove(2))
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));
            _chests.Add(Chest.Build(positions.GetAtRandomAndRemove(1).First(), data.ChestItems.GetRandomItem()));
        }

        private void AddCharactersToRoom(DungeonMapData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position, isSlept: Random.value < data.SleepChance, isShiny: Random.value < data.ShinyChance);
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
                if (Random.value < data.WeaponChanceInChest)
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

        public void AddUpStairs(DungeonMapData data, int level, Id<IEntity>? upStairsId, Id<IEntity>? upStairsDestinationId)
        {
            if (upStairsId != null && upStairsDestinationId != null)
                _stairs.Add(Stairs.Build(MovementEntityType.UpStairs, _upStairPosition, upStairsId, new(data.Name, level - 1), upStairsDestinationId));
            else
                _stairs.Add(Stairs.Build(MovementEntityType.UpStairs, _upStairPosition, new(data.Name, level - 1)));
        }

        public void AddDownStairs(DungeonMapData data, int level, Id<IEntity>? downStairsId, Id<IEntity>? downStairsDestinationId)
        {
            if (downStairsId != null && downStairsDestinationId != null)
                _stairs.Add(Stairs.Build(MovementEntityType.DownStairs, _downStairPosition, downStairsId, new(data.Name, level + 1), downStairsDestinationId));
            else
                _stairs.Add(Stairs.Build(MovementEntityType.DownStairs, _downStairPosition, new(data.Name, level + 1)));
        }

        public MapMemento Build()
        {
            return new MapMemento
            (
                tilemap: _tilemap.Serialize(),
                characters: _characters,
                items: _items,
                eventEntities: EventEntityManager.Build(_stairs, _chests, _bonfire),
                keyCharacters: _keyCharacters.Select(key => key.ToString()).ToList(),
                monsterHouse: _monsterHouse.ToOption(),
                shop: _shop.ToOption(),
                randomBlankPosition: _randomBlankPositions.GetAtRandom()
            );
        }
    }
}