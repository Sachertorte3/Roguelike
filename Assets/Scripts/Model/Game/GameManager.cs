#nullable enable
using System;
using Model.Domain.Characters.Behavior;
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
        private CharacterControllInputReceiver _receiver;

        [Inject]
        public GameManager(World world, GameInput input, CharacterControllInputReceiver receiver)
        {
            _world = world;
            _turnController = new TurnController(input);
            _receiver = receiver;
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
    }
}