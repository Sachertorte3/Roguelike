#nullable enable
using System;
using RandomDungeonWithBluePrint;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Model.Game
{
    public class GameManager
    {
        private readonly GameInput _input;
        private readonly TurnController _turnController;
        private readonly World _world;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;

        [Inject]
        public GameManager(World world, GameInput input)
        {
            _world = world;
            _input = input;
            _turnController = new TurnController(_world, _input);
            Globals.GameManager = this;
        }

        public async void LoadMap()
        {
            await _turnController.Stop();
            var bluePrint = Addressables
                .LoadAssetAsync<FieldBluePrint>(
                    "Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            _world.GenerateMap(bluePrint);
            _turnController.Run();
        }

        public void Run()
        {
            _turnController.Run();
        }
    }
}