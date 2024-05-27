#nullable enable
using Model.Game;
using Unity.Logging;
using Unity.Logging.Sinks;
using VContainer;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(GameManager gameManager, SynchronizedEventEntityView _)
        {
            LoggerInit();
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