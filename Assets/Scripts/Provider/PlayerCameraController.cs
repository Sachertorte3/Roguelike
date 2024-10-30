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
        public PlayerCameraController(World world, SynchronizedCharacterView characters, CameraFollowTarget targetCamera, CameraFlameRect rectCamera)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                if (map.Player.CurrentHp <= 0)
                {
                    targetCamera.SetPosition((Vector3Int)map.Player.CurrentPosition);
                    return;
                }

                var playerView = characters.Get(map.Player);
                targetCamera.SetTarget(playerView.gameObject);
                rectCamera.SetRect(map.TilemapViewer.Rect);
            });
        }
    }
}