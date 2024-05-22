#nullable enable
using Data;
using Model.Characters;
using Model.Entities;
using Model.Items;
using Model.Map;
using R3;
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
        public Stairs Stairs { get; init; }
        public MapManager(FieldBluePrint bluePrint, HashSet<Vector2Int> visibleArea)
        {
            _tilemap = new(bluePrint);
            CharacterManager = new();
            ItemManager = new(visibleArea);

            Stairs = new Stairs(_tilemap.GetAllPassablePositions().GetAtRandom());
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
    public class Stairs : IEntity
    {
        public Vector2Int Position => _entity.CurrentPosition;
        private Entity _entity;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Stairs(Vector2Int position)
        {
            _entity = new(position);
        }
        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
    public class EventEntity
    {
        private readonly Entity _entity;
    }
}