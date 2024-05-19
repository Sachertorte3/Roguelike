#nullable enable
using System.Collections.Generic;
using Model.Characters;
using Model.Characters.Behavior;
using Model.Setting;
using R3;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model
{
    public class World
    {
        public readonly Character Player;
        public readonly List<MapManager> Maps = new();
        public int ActiveMapIndex = 0;
        public ReadOnlyReactiveProperty<MapManager> ActiveMap => _activeMap;
        private ReactiveProperty<MapManager> _activeMap = new();
        public readonly CharacterEvents PlayerEvents = new();

        [Inject]
        public World(CharacterControllInputReceiver receiver)
        {
            Globals.World = this;
            Player = new CharacterFactory().CreatePlayer(Vector2Int.zero, receiver, Settings.IgnoreWall);
            PlayerEvents.Add(Player);
            FieldBluePrint bluePrint = Addressables.LoadAssetAsync<FieldBluePrint>("Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            GenerateMap(bluePrint);
            IsLoaded = true;
        }
        public void GenerateMap(FieldBluePrint bluePrint)
        {
            MapManager map = new(bluePrint, Player);
            Maps.Add(map);
            LoadMap(Maps.Count - 1);
        }
        private void LoadMap(int index)
        {
            _activeMap.Value = Maps[index];
            Player.Teleport(ActiveMap.CurrentValue.GetAllPassablePositions().GetAtRandom());
        }
        public bool IsLoaded { get; private set; } = false;
    }
}