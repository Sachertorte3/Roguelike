#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service;
using Domain.Service.Characters;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Map;
using UnityEngine;
using Utilities;
using static Domain.Model.DungeonData;
using Random = UnityEngine.Random;

namespace Model.Game
{
    public class MapBuilder
    {
        private readonly Tilemap _tilemap;
        private readonly List<CharacterMemento> _characters;
        private readonly List<ItemEntityMemento> _items;
        private readonly EventEntitiesMemento _eventEntities;
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;

        public MapBuilder(TilemapMemento tilemapData, SectionData data, int nextMapId, int prevMapId)
        {
            _tilemap = new Tilemap(tilemapData);
            _characters = new List<CharacterMemento>();
            _items = new List<ItemEntityMemento>();
            var chests = new List<ChestMemento>();

            var rooms = _tilemap.Rooms.ToList();

            _shop = CreateShop(data, rooms);
            _monsterHouse = CreateMonsterHouse(data, rooms);

            Vector2Int? downStairsPosition = null;
            Vector2Int? upStairsPosition = null;

            RectInt downStairsRoom = rooms.GetAtRandom();
            RectInt upStairsRoom = rooms.GetAtRandom();

            foreach (var room in rooms)
            {
                var characterCount = data.Room.CharacterCount;
                var itemCount = data.Room.ItemCount;
                var weaponCount = data.Room.WeaponCount;
                var chestCount = Random.value < data.Room.ChestChance ? 1 : 0;
                var sum = characterCount + itemCount + weaponCount + chestCount + 2;

                var positions = room.RectRange().GetAtRandom(sum).ToList();
                var characterPositions = positions.TakeAndRemove(characterCount);
                var itemPositions = positions.TakeAndRemove(itemCount);
                var weaponPositions = positions.TakeAndRemove(weaponCount);
                var chestPositions = positions.TakeAndRemove(chestCount);

                AddCharactersToRoom(data, characterPositions);
                AddItemsToRoom(data, itemPositions);
                AddWeaponsToRoom(data, weaponPositions);
                AddChestsToRoom(data, chestPositions, chests);

                if (room == downStairsRoom)
                {
                    downStairsPosition = positions.TakeAndRemove(1).First();
                }
                if (room == upStairsRoom)
                {
                    upStairsPosition = positions.TakeAndRemove(1).First();
                }
            }
            
            var downStairs = DownStairs.Build(downStairsPosition.Value, nextMapId);
            var upStairs = UpStairs.Build(upStairsPosition.Value, prevMapId);

            _eventEntities = EventEntityManager.Build(downStairs, upStairs, chests);
        }

        private ShopMemento? CreateShop(SectionData data, List<RectInt> rooms)
        {
            if (Random.value >= data.ShopChance || rooms.Count() <= 1) return null;

            var shopRoom = rooms.GetAtRandom();
            rooms.Remove(shopRoom);

            var positions = shopRoom.RectRange().GetAtRandom(6).ToList();
            foreach (var position in positions.Take(5))
                _items.Add(ItemFactory.Build(position, new Item(data.ShopItems.GetRandomItem())));

            var clerkPosition = positions.Last();
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, false, false);
            _characters.Add(clerk);
            return Shop.Build(shopRoom, clerk.EntityData, _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(SectionData data, List<RectInt> rooms)
        {
            if (Random.value >= data.MonsterHouseChance || rooms.Count() <= 1) return null;

            var monsterHouseRoom = rooms.GetAtRandom();
            rooms.Remove(monsterHouseRoom);
            
            var positions = monsterHouseRoom.RectRange().GetAtRandom(5).ToList();
            foreach (var position in positions.Take(5))
                _items.Add(ItemFactory.Build(position, new Item(data.Items.GetRandomItem())));

            return MonsterHouse.Build(monsterHouseRoom);
        }

        private void AddCharactersToRoom(SectionData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var character = CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position, Random.value < data.SleepChance, Random.value < data.ShineyChance);
                _characters.Add(character);
            }
        }

        private void AddItemsToRoom(SectionData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
                _items.Add(ItemFactory.Build(position, new Item(data.Items.GetRandomItem())));
        }

        private void AddWeaponsToRoom(SectionData data, List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                var material = data.Materials.GetRandomItem();
                var mold = data.WeaponMolds.GetRandomItem();
                if (Random.value < data.PrefixChance)
                {
                    var prefix = data.WeaponPrefixes.GetRandomItem();
                    var weapon = WeaponFactory.Create(prefix, material, mold);
                    _items.Add(ItemFactory.Build(position, new Item(weapon)));
                }
                else
                {
                    var weapon = WeaponFactory.Create(material, mold);
                    _items.Add(ItemFactory.Build(position, new Item(weapon)));
                }
            }
        }

        private void AddChestsToRoom(SectionData data, List<Vector2Int> positions, List<ChestMemento> chests)
        {
            foreach (var position in positions)
            {
                var material = data.Materials.GetRandomItem();
                var mold = data.WeaponMolds.GetRandomItem();
                var prefix = data.WeaponPrefixes.GetRandomItem();
                var weapon = WeaponFactory.Create(prefix, material, mold);
                chests.Add(Chest.Build(position, weapon));
            }
        }

        public MapMemento Build()
        {
            return new MapMemento(
                _tilemap.Serialize(),
                _characters,
                _items,
                _eventEntities,
                _monsterHouse,
                _shop
            );
        }
    }
}