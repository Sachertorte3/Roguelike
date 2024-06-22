#nullable enable
using System;
using Model.Domain.Events;
using Unity.Logging;
using VContainer;

namespace Model.Game
{
    public class GameManager : IGameManager
    {
        private readonly World _world;
        private TurnController _turnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;

        [Inject]
        public GameManager(World world, GameInput input)
        {
            _world = world;
            _turnController = new TurnController(input);
            Globals.GameManager = this;
        }

        public async void LoadMap(int mapId)
        {
            Log.Debug("Start LoadMap");
            await _turnController.Stop();
            var map = _world.LoadMap(mapId);
            _turnController.Run(map);
            Log.Debug("End LoadMap");
        }
    }
}