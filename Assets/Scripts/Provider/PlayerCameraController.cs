#nullable enable
using Model;
using Utilities;
using VContainer;

namespace Provider
{
    public class PlayerCameraController
    {
        [Inject]
        public PlayerCameraController(World world, SynchronizedCharacterView characters, CameraFollowTarget camera)
        {
            var playerView = characters.Get(world.Player);

            camera.SetTarget(playerView.gameObject);
        }
    }
}