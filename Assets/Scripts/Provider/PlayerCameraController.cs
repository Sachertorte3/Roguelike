#nullable enable
using Model.Game;
using Utilities;
using VContainer;

namespace Provider
{
    public class PlayerCameraController
    {
        [Inject]
        public PlayerCameraController(World world, SynchronizedCharacterView characters, CameraFollowTarget camera)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                var playerView = characters.Get(map.Player);
                camera.SetTarget(playerView.gameObject);
            });
        }
    }
}