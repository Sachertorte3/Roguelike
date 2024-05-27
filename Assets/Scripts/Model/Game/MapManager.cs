#nullable enable
using System.Collections.Generic;
using Data;
using Model.Domain;
using Model.Domain.Items;
using Model.Domain.Map;
using ObservableCollections;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Game
{
    public class MapManager : IMapViewer
    {
        private readonly Tilemap _tilemap;

        public MapManager(FieldBluePrint bluePrint, HashSet<Vector2Int> visibleArea)
        {
            _tilemap = new Tilemap(bluePrint);
            CharacterManager = new CharacterManager();
            ItemManager = new ItemManager(visibleArea);

            EventEntities = new ObservableHashSet<IEventEntity>
                { new Stairs(_tilemap.GetAllPassablePositions().GetAtRandom()) };
        }

        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }
        public ObservableHashSet<IEventEntity> EventEntities { get; init; }
        public ITilemapViewer Tilemap => _tilemap;

        public void Spawn(IWorld world)
        {
            var data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
            foreach (var position in _tilemap.GetAllPassablePositions().GetAtRandom(10))
                CharacterManager.SpawnCharacter(data.Enemies.GetAtRandom(), position, world);
            foreach (var position in _tilemap.GetAllPassablePositions().GetAtRandom(30))
                ItemManager.SpawnItem(new Item(data.Items.GetAtRandom()), position);
        }

        public void Load()
        {
        }

        public void UnLoad()
        {
        }
    }
}