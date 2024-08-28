#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Unity.Logging;
using UnityEngine;
using VContainer;
using static Model.Game.World;

namespace Model.Game
{
    public class GameManager : IGameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        private readonly ChoiceReceiver _choiceReceiver;
        private readonly CharacterControlInputReceiver _receiver;
        private readonly DungeonBluePrintData _dungeonBluePrintData;

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver, CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonBluePrintData)
        {
            _world = world;
            _turnController = new TurnController(input);
            _choiceReceiver = choiceReceiver;
            _receiver = receiver;
            _dungeonBluePrintData = dungeonBluePrintData;
            Globals.GameManager = this;
        }

        public async UniTask<int> GetChoice(string text, params string[] choices)
        {
            return await _choiceReceiver.GetChoice(text, choices);
        }

        public async void LoadMap(int mapId)
        {
            Log.Debug("Start LoadMap");
            _receiver.Enable(false);
            await _turnController.Stop();
            var map = _world.LoadMap(mapId);
            _turnController.Run(map);
            _receiver.Enable(true);
            Log.Debug("End LoadMap");
        }

        public void Save()
        {
            Log.Debug("Start Save");
            var saveData = _world.SerializeSaveData();
            var saveDataStr = JsonUtility.ToJson(saveData);
            System.IO.File.WriteAllText("save.json", saveDataStr);
            var updatedMaps = _world.SerializeUpdatedMaps();
            foreach (var map in updatedMaps)
            {
                var mapStr = JsonUtility.ToJson(map.Value);
                System.IO.File.WriteAllText($"map_{map.Key}.json", mapStr);
            }
            Log.Debug("End Save");
        }

        public async void Load()
        {
            Log.Debug("Start Load");
            _receiver.Enable(false);
            await _turnController.Stop();
            MapManager map = null;
            if (System.IO.File.Exists("save.json"))
            {
                var str = System.IO.File.ReadAllText("save.json");
                var saveData = JsonUtility.FromJson<SaveData>(str);
                var maps = new Dictionary<int, MapMemento>();
                foreach (var mapId in saveData.MapIds)
                {
                    var mapStr = System.IO.File.ReadAllText($"map_{mapId}.json");
                    var mapMemento = JsonUtility.FromJson<MapMemento>(mapStr);
                    maps.Add(mapId, mapMemento);
                }
                map = _world.LoadWorld(Build(saveData, maps));
            }
            else
            {
                _world.CreateNew(_dungeonBluePrintData);
                map = _world.LoadMap(1);
            }
            _turnController.Run(map);
            _receiver.Enable(true);
            Log.Debug("End Load");
        }
    }
}