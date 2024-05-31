#nullable enable
using System;
using Unity.Logging;
using RandomDungeonWithBluePrint;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Model.Game
{
    public class GameManager
    {
        private TurnController _turnController;
        private readonly World _world;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;

        [Inject]
        public GameManager(World world, GameInput input)
        {
            _world = world;
            _turnController = new(input);
            Globals.GameManager = this;
        }

        public async void LoadMap()
        {
            Log.Debug("Start LoadMap");
            await _turnController.Stop();
            var bluePrint = Addressables
                .LoadAssetAsync<FieldBluePrint>(
                    "Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            var map = _world.GenerateMap(bluePrint);
            _turnController.Run(map);
            Log.Debug("End LoadMap");
        }
    }
}