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

        public MapBuilder(TilemapMemento tilemapData, SectionData data, int nextMapId, int? prevMapId)
        {
            _tilemap = new Tilemap(tilemapData);
            _characters = new List<CharacterMemento>();
            _items = new List<ItemEntityMemento>();
            var chests = new List<ChestMemento>();

            var rooms = _tilemap.Rooms.ToList();

            _shop = CreateShop(data, rooms);
            _monsterHouse = CreateMonsterHouse(data, rooms);

            PopulateRooms(data, rooms, chests);

            var downStairs = DownStairs.Build(_tilemap.GetAllPassablePositions().GetAtRandom(), nextMapId);
            var upStairs = prevMapId.HasValue
                ? UpStairs.Build(_tilemap.GetAllPassablePositions().GetAtRandom(), prevMapId.Value)
                : null;

            _eventEntities = EventEntityManager.Build(downStairs, upStairs, chests);
        }

        private ShopMemento? CreateShop(SectionData data, List<RectInt> rooms)
        {
            if (Random.value >= data.ShopChance || !rooms.Any()) return null;

            var shopRoom = rooms.GetAtRandom();
            rooms.Remove(shopRoom);

            foreach (var position in shopRoom.RectRange().GetAtRandom(2))
                _items.Add(ItemFactory.Build(position, new Item(data.Items.GetRandomItem())));

            var clerk = CharacterFactory.BuildCharacter(data.Clerk, shopRoom.RectRange().GetAtRandom(), Random.value < data.ShineyChance);
            _characters.Add(clerk);
            return Shop.Build(shopRoom, clerk.EntityData, _items.ToList());
        }

        private RoomMemento? CreateMonsterHouse(SectionData data, List<RectInt> rooms)
        {
            if (Random.value >= data.MonsterHouseChance || !rooms.Any()) return null;

            var monsterHouseRoom = rooms.GetAtRandom();
            rooms.Remove(monsterHouseRoom);

            return MonsterHouse.Build(monsterHouseRoom);
        }

        private void PopulateRooms(SectionData data, List<RectInt> rooms, List<ChestMemento> chests)
        {
            foreach (var room in rooms)
            {
                AddCharactersToRoom(data, room);
                AddItemsToRoom(data, room);
                AddWeaponsToRoom(data, room);
                AddChestsToRoom(data, room, chests);
            }
        }

        private void AddCharactersToRoom(SectionData data, RectInt room)
        {
            foreach (var position in room.RectRange().GetAtRandom(2))
                _characters.Add(CharacterFactory.BuildCharacter(data.Enemies.GetRandomItem(), position, Random.value < data.ShineyChance));
        }

        private void AddItemsToRoom(SectionData data, RectInt room)
        {
            foreach (var position in room.RectRange().GetAtRandom(2))
                _items.Add(ItemFactory.Build(position, new Item(data.Items.GetRandomItem())));
        }

        private void AddWeaponsToRoom(SectionData data, RectInt room)
        {
            foreach (var position in room.RectRange().GetAtRandom(1))
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

        private void AddChestsToRoom(SectionData data, RectInt room, List<ChestMemento> chests)
        {
            foreach (var position in room.RectRange().GetAtRandom(1))
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