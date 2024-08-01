#nullable enable
using System;
using Domain.Model;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Unity.Logging;
using VContainer;
using UnityEngine;

namespace Model.Game
{
    public class GameManager : IGameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        private readonly CharacterControlInputReceiver _receiver;
        private readonly DungeonBluePrintData _dungeonBluePrintData;

        [Inject]
        public GameManager(World world, GameInput input, CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonBluePrintData)
        {
            _world = world;
            _turnController = new TurnController(input);
            _receiver = receiver;
            _dungeonBluePrintData = dungeonBluePrintData;
            Globals.GameManager = this;
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
            var world = _world.Serialize();
            var str = JsonUtility.ToJson(world);
            System.IO.File.WriteAllText("world_data.json", str);
        }

        public async void Load()
        {
            Log.Debug("Start Load");
            _receiver.Enable(false);
            await _turnController.Stop();
            MapManager map = null;
            if (System.IO.File.Exists("world_data.json"))
            {
                var str = System.IO.File.ReadAllText("world_data.json");
                var world = JsonUtility.FromJson<WorldMemento>(str);
                map = _world.LoadWorld(world);
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