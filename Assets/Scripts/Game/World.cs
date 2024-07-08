#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service;
using Domain.Service.Characters.Behavior;
using Domain.Service.Map;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using static Domain.Model.DungeonData;

namespace Model.Game
{
    public class World
    {
        private ReactiveProperty<MapManager?> _activeMap = new();
        private int _activeMapId = 0;
        private DungeonData _dungeonData;
        private Dictionary<int, MapMemento> _maps = new();
        private CharacterControllInputReceiver _receiver;
        public int ActiveMapIndex = 0;

        [Inject]
        public World(CharacterControllInputReceiver receiver)
        {
            Globals.World = this;
            _receiver = receiver;

            _dungeonData = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset")
                .WaitForCompletion();
        }

        public ReadOnlyReactiveProperty<MapManager?> ActiveMap => _activeMap;

        private SectionData GetSectionData(int level)
        {
            var currentDepth = 0;
            foreach (var section in _dungeonData.Sections)
            {
                currentDepth += section.Depth;
                if (level <= currentDepth)
                {
                    return section;
                }
            }

            throw new InvalidOperationException("指定されたレベルに対応するセクションが見つかりません。");
        }

        private MapMemento GetMapMemento(int mapId)
        {
            if (_maps.ContainsKey(mapId))
            {
                return _maps[mapId];
            }
            else
            {
                var sectionData = GetSectionData(mapId);
                return MapManager.Build(Tilemap.Build(sectionData.Field), sectionData, mapId + 1,
                    mapId > 1 ? mapId - 1 : null);
            }
        }

        public MapManager LoadMap(int mapId)
        {
            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(mapId);

            CharacterMemento? playerData = null;
            List<CharacterMemento>? characters = null;
            Vector2Int? initialPosition = null;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                characters = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize())
                    .ToList();
                if (_activeMapId < mapId) // 下り階段から上り階段へ
                {
                    if (mapMemento.EventEntities.UpStairs == null)
                        throw new InvalidOperationException("upstairs is null");
                    initialPosition = mapMemento.EventEntities.UpStairs.Entity.Position;
                }
                else if (_activeMapId > mapId) // 上り階段から下り階段へ
                {
                    initialPosition = mapMemento.EventEntities.DownStairs.Entity.Position;
                }

                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, GetSectionData(mapId), playerData, characters, initialPosition, _receiver);

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