#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service;
using Domain.Service.Characters;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Map;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Model.Game
{
    public class MapBuilder
    {
        private readonly Tilemap _tilemap;
        private readonly List<CharacterMemento> _characters;
        private readonly List<ItemEntityMemento> _items;
        private readonly List<StairsMemento> _stairs;
        private readonly List<ChestMemento> _chests;
        private readonly List<Id<IEntity>> _keyCharacters;
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly Vector2Int _upStairPosition;
        private readonly Vector2Int _downStairPosition;

        public MapBuilder(TilemapMemento tilemapData, DungeonMapData data)
        {
            _tilemap = new(tilemapData);
            _characters = new();
            _items = new();
            _keyCharacters = new();
            _stairs = new();
            _chests = new();

            var rooms = _tilemap.Rooms.ToList();

            _shop = CreateShop(data, rooms);
            _monsterHouse = CreateMonsterHouse(data, rooms);

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
                var sum = characterCount + itemCount + weaponCount + chestCount + bossCount + 2;

                var positions = room.RectRange().GetAtRandom(sum).ToList();
                var characterPositions = positions.TakeAndRemove(characterCount);
                var itemPositions = positions.TakeAndRemove(itemCount);
                var weaponPositions = positions.TakeAndRemove(weaponCount);
                var chestPositions = positions.TakeAndRemove(chestCount);

                AddCharactersToRoom(data, characterPositions);
                AddItemsToRoom(data, itemPositions);
                AddWeaponsToRoom(data, weaponPositions);
                AddChestsToRoom(data, chestPositions, _chests);

                if (room == bossRoom)
                {
                    foreach (var bossData in data.Boss)
                    {
                        var boss = CharacterFactory.BuildCharacter(bossData, positions.TakeAndRemove(1).First(), isSlept: false, isShiny: false);
                        _characters.Add(boss);
                        _keyCharacters.Add(new(boss.Entity.Id));
                    }
                }
                if (room == downStairsRoom)
                {
                    _downStairPosition = positions.TakeAndRemove(1).First();
                }
                if (room == upStairsRoom)
                {
                    _upStairPosition = positions.TakeAndRemove(1).First();
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

            var shopItems = data.ShopItems.GetRandomItem().Items;

            var positions = shopRoom.RectRange().GetAtRandom(6).ToList();
            foreach (var position in positions.Take(5))
                _items.Add(ItemFactory.Build(position, Item.Build(shopItems.GetRandomItem(), ItemState.ShopItem)));

            var clerkPosition = positions.Last();
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false, hasHomePosition: true);
            _characters.Add(clerk);
            return Shop.Build(shopRoom, clerk.Entity, _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(DungeonMapData data, List<RectInt> rooms)
        {
            if (Random.value >= data.MonsterHouseChance || rooms.Count() <= 1) return null;

            var monsterHouseRoom = rooms.GetAtRandom();
            rooms.Remove(monsterHouseRoom);

            var positions = monsterHouseRoom.RectRange().GetAtRandom(5).ToList();
            foreach (var position in positions.Take(5))
                _items.Add(ItemFactory.Build(position, Item.Build(data.Items.GetRandomItem())));

            return MonsterHouse.Build(monsterHouseRoom);
        }

        private void AddCharactersToRoom(DungeonMapData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position, Random.value < data.SleepChance, Random.value < data.ShinyChance);
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
            {
                Tilemap = _tilemap.Serialize(),
                Characters = _characters,
                Items = _items,
                EventEntities = EventEntityManager.Build(_stairs, _chests),
                KeyCharacters = _keyCharacters.Select(key => key.ToString()).ToList(),
                MonsterHouse = new(_monsterHouse),
                Shop = new(_shop)
            };
        }
    }
}