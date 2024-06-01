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
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model.Game
{
    public class World
    {
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

        public MapManager GenerateMap(FieldBluePrint bluePrint)
        {
            var tilemap = Tilemap.BuildMemento(bluePrint);
            return LoadMap(tilemap);
        }
        public MapManager LoadMap(TilemapMemento tilemap)
        {
            CharacterMemento? playerData = null;
            if (_activeMap.CurrentValue != null)
            {
                _activeMap.CurrentValue.Dispose();
                playerData = _activeMap.CurrentValue.Player.Serialize();
            }
            MapManager map = new(_receiver, tilemap, _activeMap.CurrentValue?.Tilemap.Serialize(), playerData);

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