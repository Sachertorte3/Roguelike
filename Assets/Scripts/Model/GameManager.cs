#nullable enable
using System;
using VContainer;

namespace Model
{
    public class GameManager
    {
        private readonly World _world;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;

        [Inject]
        public GameManager(World world)
        {
            _world = world;
        }

        public void LoadMap()
        {

        }

        public void Run()
        {
            new TurnController(_world);
        }
    }
}