#nullable enable
using Model;
using Model.Game;
using Utilities;
using VContainer;
using R3;

namespace Provider
{
    public class PlayerCameraController
    {
        [Inject]
        public PlayerCameraController(World world, SynchronizedCharacterView characters, CameraFollowTarget camera)
        {
            world.ActiveMap.SubscribeToAll(map =>
            {
                var playerView = characters.Get(map.Player);
                camera.SetTarget(playerView.gameObject);
            });
        }
    }
}