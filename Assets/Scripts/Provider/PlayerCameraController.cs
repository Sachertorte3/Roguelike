#nullable enable
using Game;
using UnityEngine;
using Utilities;
using VContainer;
using R3;

namespace Provider
{
    public class PlayerCameraController
    {
        [Inject]
        public PlayerCameraController(World world, SynchronizedCharacterView characters,
            CameraFollowTarget targetCamera, CameraFlameRect rectCamera)
        {
            var disposable = new SerialDisposable();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                if (map.Player.Character.IsDead)
                {
                    targetCamera.SetPosition((Vector3Int)map.Player.Character.Entity.CurrentPosition);
                    return;
                }

                var playerView = characters.Get(map.Player.Character);
                targetCamera.SetTarget(playerView.gameObject);
                disposable.Disposable = map.TilemapViewer.Rect.Subscribe(rect => rectCamera.SetRect(rect));
            });
        }
    }
}