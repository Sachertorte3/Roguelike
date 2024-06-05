#nullable enable
using System.Collections.Generic;
using System.Linq;
using Data.Character;
using Data.Map;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Map;
using R3;
using RandomDungeonWithBluePrint;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        private MapMemento GetMapMemento(int mapId)
        {
            if (_maps.ContainsKey(mapId))
            {
                return _maps[mapId];
            }
            else
            {
                var bluePrint = Addressables
                .LoadAssetAsync<FieldBluePrint>(
                    "Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
                return MapManager.Build(Tilemap.BuildMemento(bluePrint), mapId + 1, mapId > 0 ? mapId - 1 : null);
            }
        }
        public MapManager LoadMap(int mapId)
        {
            Log.Debug($"LoadMap {mapId}");
            var mapMemento = GetMapMemento(mapId);

            CharacterMemento? playerData = null;
            List<CharacterMemento>? characters = null;
            Vector2Int? initialPosition = null;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                characters = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize()).ToList();
                if (_activeMapId < mapId) // 下り階段から上り階段へ
                {
                    initialPosition = mapMemento.UpStairs.Entity.Position;
                }
                else if (_activeMapId > mapId) // 上り階段から下り階段へ
                {
                    initialPosition = mapMemento.DownStairs.Entity.Position;
                }
                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, playerData, characters, initialPosition, _receiver);

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