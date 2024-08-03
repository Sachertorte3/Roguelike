#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Map;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Model.Game
{
    public class World : ISerializable<WorldMemento>
    {
        private ReactiveProperty<MapManager?> _activeMap = new();
        private int _activeMapId = 0;
        private DungeonBluePrintData _dungeonData;
        private Dictionary<int, MapMemento> _maps = new();
        private HashSet<int> _updatedMapIds = new();
        private CharacterControlInputReceiver _receiver;

        [Inject]
        public World(CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonData)
        {
            Globals.World = this;
            _receiver = receiver;
            _dungeonData = dungeonData;
        }

        public void CreateNew(DungeonBluePrintData dungeonData)
        {
            _dungeonData = dungeonData;
            _maps = new();
            _activeMapId = 0;
            _updatedMapIds = new();
            _activeMap.Value = null;
        }

        public MapManager LoadWorld(WorldMemento memento)
        {
            _dungeonData = Addressables.LoadAssetAsync<DungeonBluePrintData>($"Assets/Database/DungeonBluePrintData/{memento.DungeonDataName}.asset").WaitForCompletion();
            _maps = memento.Maps;
            
            var mapId = memento.ActiveMapId;
            _updatedMapIds = new HashSet<int> { mapId };

            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(mapId);

            if (_activeMap.CurrentValue != null)
            {
                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeonData.CreateMapData(mapId), memento.Player, new(), memento.Player.Entity.Position, _receiver, mapId);

            _activeMapId = mapId;
            _activeMap.Value = map;

            return map;
        }

        public WorldMemento Serialize()
        {
            _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
            var playerData = _activeMap.CurrentValue.Player.Serialize();
            return new WorldMemento
            {
                DungeonDataName = _dungeonData.name,
                Player = playerData,
                Maps = new(_maps),
                ActiveMapId = _activeMapId
            };
        }

        [Serializable]
        public class SaveData
        {
            public string DungeonDataName;
            public CharacterMemento Player;
            public int ActiveMapId;
            public List<int> MapIds;
        }

        public SaveData SerializeSaveData()
        {
            return new SaveData
            {
                DungeonDataName = _dungeonData.name,
                Player = _activeMap.CurrentValue.Player.Serialize(),
                ActiveMapId = _activeMapId,
                MapIds = _maps.Keys.ToList()
            };
        }

        public Dictionary<int, MapMemento> SerializeUpdatedMaps()
        {
            _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
            var updatedMaps = _updatedMapIds.ToDictionary(mapId => mapId, mapId => _maps[mapId]);
            _updatedMapIds = new HashSet<int> { _activeMapId };
            return updatedMaps;
        }

        public static WorldMemento Build(SaveData saveData, Dictionary<int, MapMemento> maps)
        {
            if (saveData.MapIds.Except(maps.Keys).Any())
                throw new ArgumentException("Map count is not match");
            return new WorldMemento
            {
                DungeonDataName = saveData.DungeonDataName,
                Player = saveData.Player,
                Maps = new(maps),
                ActiveMapId = saveData.ActiveMapId
            };
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
                var dungeonData = _dungeonData.CreateMapData(mapId);
                return new MapBuilder(Tilemap.Build(dungeonData.Field), dungeonData, mapId + 1, mapId - 1).Build();
            }
        }

        public MapManager LoadMap(int mapId)
        {
            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(mapId);
            _updatedMapIds.Add(mapId);

            CharacterMemento? playerData = null;
            List<CharacterMemento>? characters = null;
            Vector2Int initialPosition = mapMemento.EventEntities.UpStairs.Entity.Position;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                characters = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize())
                    .ToList();
                if (_activeMapId < mapId) // 下り階段から上り階段へ
                {
                    initialPosition = mapMemento.EventEntities.UpStairs.Entity.Position;
                }
                else if (_activeMapId > mapId) // 上り階段から下り階段へ
                {
                    initialPosition = mapMemento.EventEntities.DownStairs.Entity.Position;
                }

                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeonData.CreateMapData(mapId), playerData, characters, initialPosition, _receiver, mapId);

            _activeMapId = mapId;
            _activeMap.Value = map;
            return map;
        }

        public HashSet<ICharacter> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            if (ActiveMap.CurrentValue == null)
                throw new InvalidOperationException("ActiveMap is null");
            return ActiveMap.CurrentValue.GetCharactersInArea(area);
        }

        public void HandleItemDrop(int inventoryIndex)
        {
            if (ActiveMap.CurrentValue == null)
                throw new InvalidOperationException("ActiveMap is null");
            ActiveMap.CurrentValue.HandleItemDrop(inventoryIndex);
        }
    }
}