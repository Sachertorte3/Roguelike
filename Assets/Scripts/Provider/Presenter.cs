#nullable enable
using Model;
using R3;
using Sirenix.Utilities;
using System.Linq;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using Utilities.ObjectsManager;
using VContainer;
using View;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(TileMaskController tileMask, GameManager gameManager, World world)
        {
            LoggerInit();

            world.PlayerEvents.OnVisibleAreaChanged.Subscribe(area =>
            {
                tileMask.SetTilesTranslucent(area.AreaExited);
                tileMask.SetTilesVisible(area.AreaEntered);
                ObjectsManager.GetObjectsByType<SpriteView>()
                    .Where(view => area.AreaExited.Contains(Vector2Int.RoundToInt(view.Position())))
                    .ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>()
                    .Where(view => area.AreaEntered.Contains(Vector2Int.RoundToInt(view.Position())))
                    .ForEach(view => view.SetVisibility(true));
            });

            world.ActiveMap.Subscribe(mapLoaded =>
            {
                world.Player.Area.Refrash(world.Player.CurrentPosition);
            });
            world.Player.Area.Refrash(world.Player.CurrentPosition);

            ObjectsManager.ObserveAdd<SpriteView>().Subscribe(view =>
                view.SetVisibility(world.Player.Area.Get()
                    .Contains(Vector2Int.RoundToInt(view.Position()))));

            gameManager.Run();
        }

        private void LoggerInit()
        {
            Log.Logger = new Logger(new LoggerConfig()
                .MinimumLevel.Debug()
                .OutputTemplate("[{Timestamp}] {Level} | {Message}{NewLine}{Stacktrace}")
                .WriteTo.UnityDebugLog());
            Log.Debug("Init Logger");
        }
    }
}