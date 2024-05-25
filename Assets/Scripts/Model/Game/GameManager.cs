#nullable enable
using Model.Characters;
using RandomDungeonWithBluePrint;
using System;
using UnityEngine.AddressableAssets;
using VContainer;
using R3;
using Model.Game;

namespace Model
{
    public class GameManager
    {
        private readonly World _world;
        private readonly GameInput _input;
        private readonly TurnController _turnController;
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
            FieldBluePrint bluePrint = Addressables.LoadAssetAsync<FieldBluePrint>("Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            _world.GenerateMap(bluePrint);
            _turnController.Run();
        }

        public void Run()
        {
            _turnController.Run();
        }
    }
}