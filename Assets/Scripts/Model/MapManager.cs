#nullable enable
using Data;
using Model.Characters;
using Model.Entities;
using Model.Items;
using Model.Map;
using ObservableCollections;
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
        public ObservableHashSet<IEventEntity> EventEntities { get; init; }
        public MapManager(FieldBluePrint bluePrint, HashSet<Vector2Int> visibleArea)
        {
            _tilemap = new(bluePrint);
            CharacterManager = new();
            ItemManager = new(visibleArea);

            EventEntities = new ObservableHashSet<IEventEntity> { new Stairs(_tilemap.GetAllPassablePositions().GetAtRandom()) };
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
    public interface IEventEntity : IHasEvent, IEntity
    {
        public Sprite Icon { get; }
    }
    public interface IHasEvent
    {
        public void DoEvent();
    }
    public class Stairs : IEventEntity
    {
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        private Entity _entity;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/MapChipPalettes/Tiles/tiles.png[tiles_42]").WaitForCompletion();
        public Stairs(Vector2Int position)
        {
            _entity = new(position);
        }
        public void DoEvent()
        {
            Globals.GameManager.LoadMap();
        }
        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
}