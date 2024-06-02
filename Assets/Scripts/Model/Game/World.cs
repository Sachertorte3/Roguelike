#nullable enable
using System.Collections.Generic;
using System.Linq;
using Data.Character;
using Data.Map;
using Data.Setting;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Items;
using Model.Domain.Map;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model.Game
{
    public class World
    {
        private Dictionary<int, MapMemento> _maps = new();
        private int _activeMapId = 0;
        private ReactiveProperty<MapManager?> _activeMap = new();
        public int ActiveMapIndex = 0;
        private CharacterControllInputReceiver _receiver;

        [Inject]
        public World(CharacterControllInputReceiver receiver)
        {
            Globals.World = this;
            _receiver = receiver;
        }

        public ReadOnlyReactiveProperty<MapManager?> ActiveMap => _activeMap;

        private TilemapMemento GetMapMemento(int mapId)
        {
            if (_maps.ContainsKey(mapId))
            {
                return _maps[mapId].Tilemap;
            }
            else
            {
                var bluePrint = Addressables
                .LoadAssetAsync<FieldBluePrint>(
                    "Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
                return Tilemap.BuildMemento(bluePrint);
            }
        }
        public MapManager LoadMap(int mapId)
        {
            Log.Debug($"LoadMap {mapId}");
            var tilemap = GetMapMemento(mapId);

            CharacterMemento? playerData = null;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
                _activeMap.CurrentValue.Dispose();
                playerData = _activeMap.CurrentValue.Player.Serialize();
            }
            MapManager map = new(_receiver, tilemap, mapId+1, mapId>0? mapId-1 : null, playerData);

            _activeMapId = mapId;
            _activeMap.Value = map;
            return map;
        }

        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return ActiveMap.CurrentValue.GetCharactersInArea(area);
        }
        public void HandleItemDrop(int inventoryIndex)
        {
            ActiveMap.CurrentValue.HandleItemDrop(inventoryIndex);
        }
    }
}