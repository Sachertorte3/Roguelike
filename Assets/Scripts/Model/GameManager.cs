#nullable enable
using RandomDungeonWithBluePrint;
using System;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Model
{
    public class GameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;

        [Inject]
        public GameManager(World world)
        {
            _world = world;
            _turnController = new TurnController(_world);
            Globals.GameManager = this;
        }

        public void LoadMap()
        {
            _turnController.Stop();
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