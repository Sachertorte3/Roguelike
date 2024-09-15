#nullable enable
using Game;
using UnityEngine;
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
                if (map.Player.CurrentHp <= 0)
                {
                    camera.SetPosition((Vector3Int)map.Player.CurrentPosition);
                    return;
                }
                var playerView = characters.Get(map.Player);
                camera.SetTarget(playerView.gameObject);
            });
        }
    }
}