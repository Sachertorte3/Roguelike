#nullable enable
using Data;
using Model.Characters;
using Model.Items;
using Model.Map;
using RandomDungeonWithBluePrint;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model
{
    public class MapManager : IMapViewer
    {
        public ITilemapViewer Tilemap => _tilemap;
        private readonly Tilemap _tilemap;
        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }
        public MapManager(FieldBluePrint bluePrint, HashSet<Vector2Int> visibleArea)
        {
            _tilemap = new(bluePrint);
            CharacterManager = new();
            ItemManager = new(visibleArea);
        }
        public void Spawn()
        {
            foreach (var position in _tilemap.GetAllPassablePositions().GetAtRandom(10))
                CharacterManager.SpawnCharacter(position);
            var data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
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