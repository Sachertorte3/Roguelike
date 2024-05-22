#nullable enable
using Model;
using R3;
using Unity.Logging;
using Unity.Logging.Sinks;
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
                tileMask.SetTilesTranslucent(area.Message.AreaExited);
                tileMask.SetTilesVisible(area.Message.AreaEntered);
            });
            tileMask.SetTilesVisible(world.Player.Area.VisibleArea);

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